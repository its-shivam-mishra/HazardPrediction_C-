using System.Text.Json;
using Microsoft.Azure.Cosmos;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class LocalWeatherService : ILocalWeatherService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalWeatherService> _logger;

        public LocalWeatherService(IWebHostEnvironment env, CosmosClient cosmosClient, IConfiguration configuration, ILogger<LocalWeatherService> logger)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(List<string> Hazards, string PredictionId)> GetHazardsForCityAsync(string city)
        {
            var hazards = new List<string>();
            string predictionId = string.Empty;

            // 1. Try Cosmos DB
            try
            {
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);
                    // Filter out already sent notifications and ensure it's a weather record
                    var query = new QueryDefinition("SELECT * FROM c WHERE c.city = @city AND (c.Type = 'Weather' OR NOT IS_DEFINED(c.Type)) AND (c.isNotificationSent = false OR NOT IS_DEFINED(c.isNotificationSent)) ORDER BY c._ts DESC OFFSET 0 LIMIT 1")
                        .WithParameter("@city", city);

                    using var iterator = container.GetItemQueryIterator<UnifiedWeatherResponse>(query);
                    if (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        var weatherData = response.FirstOrDefault();

                        if (weatherData != null && weatherData.HazardPrediction != null)
                        {
                            predictionId = weatherData.Id; // Capture the ID
                            var prediction = weatherData.HazardPrediction;
                            if (prediction.FireRiskPercent > 20) hazards.Add("Fire");
                            if (prediction.FloodRiskPercent > 20) hazards.Add("Flood");
                            if (prediction.StormRiskPercent > 20) hazards.Add("Storm");
                            if (prediction.HeatWaveRiskPercent > 20) hazards.Add("Heat Wave");
                            if (prediction.SnowRiskPercent > 20) hazards.Add("Snow");

                            _logger.LogInformation("Fetched hazards for {City} from Cosmos DB", city);
                            return (hazards, predictionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hazards from Cosmos DB for {City}. Falling back to local files.", city);
            }



            return (hazards, predictionId);


        }

        public async Task<List<UnifiedWeatherResponse>> GetActiveHazardsAsync()
        {
            var activeHazards = new List<UnifiedWeatherResponse>();
            try
            {
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);
                    // Fetch all unsent weather records
                    // Note: We fetch all because we need to group by city client-side to find the latest
                    var query = new QueryDefinition("SELECT * FROM c WHERE (c.Type = 'Weather' OR NOT IS_DEFINED(c.Type)) AND (c.isNotificationSent = false OR NOT IS_DEFINED(c.isNotificationSent))");

                    using var iterator = container.GetItemQueryIterator<UnifiedWeatherResponse>(query);
                    var allResults = new List<UnifiedWeatherResponse>();

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        allResults.AddRange(response);
                    }

                    // Return all unsent records, ordered by date (newest first)
                    activeHazards = allResults
                        .OrderByDescending(w => w.PredictionGeneratedDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active hazards from Cosmos DB.");
            }

            // DEBUG: Check raw JSON stream
            try
            {
                Console.WriteLine("DEBUG BLOCK REACHED");
                var dbId = _configuration["Cosmos:DatabaseId"];
                var contId = _configuration["Cosmos:ContainerId"];
                if (!string.IsNullOrEmpty(dbId) && !string.IsNullOrEmpty(contId))
                {
                    var container = _cosmosClient.GetContainer(dbId, contId);
                    var debugQuery = new QueryDefinition("SELECT TOP 1 * FROM c");
                    using var debugIterator = container.GetItemQueryStreamIterator(debugQuery);
                    if (debugIterator.HasMoreResults)
                    {
                        using var response = await debugIterator.ReadNextAsync();
                        if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content != null)
                        {
                            using var reader = new StreamReader(response.Content);
                            var json = await reader.ReadToEndAsync();
                            _logger.LogInformation($"DEBUG RAW STREAM: {json}");
                            Console.WriteLine($"DEBUG RAW STREAM CONSOLE: {json}");
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.LogError($"Debug stream query failed: {ex.Message}"); }

            return activeHazards;
        }

        public async Task<UnifiedWeatherResponse?> GetWeatherRecordAsync(string id, string city)
        {
            try
            {
                Console.WriteLine($"DEBUG: GetWeatherRecordAsync called for Id: {id}, City: {city}");
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);

                    // Use Query instead of ReadItemAsync to avoid potential PartitionKey issues
                    var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                        .WithParameter("@id", id);

                    using var iterator = container.GetItemQueryIterator<UnifiedWeatherResponse>(query);
                    if (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        var weatherRecord = response.FirstOrDefault();
                        Console.WriteLine($"DEBUG: GetWeatherRecordAsync success. Found: {weatherRecord != null}");
                        return weatherRecord;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather record {Id} for {City}", id, city);
                Console.WriteLine($"DEBUG: GetWeatherRecordAsync Error: {ex.Message}");
            }
            return null;
        }
    }
}
