using System.Text.Json;
using Microsoft.Azure.Cosmos;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class LocalWeatherService : ILocalWeatherService
    {
        private readonly string _responsePath;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalWeatherService> _logger;

        public LocalWeatherService(IWebHostEnvironment env, CosmosClient cosmosClient, IConfiguration configuration, ILogger<LocalWeatherService> logger)
        {
            _responsePath = Path.Combine(env.ContentRootPath, "response");
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<string>> GetHazardsForCityAsync(string city)
        {
            var hazards = new List<string>();

            // 1. Try Cosmos DB
            try
            {
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);
                    var query = new QueryDefinition("SELECT * FROM c WHERE c.city = @city ORDER BY c._ts DESC OFFSET 0 LIMIT 1")
                        .WithParameter("@city", city);

                    using var iterator = container.GetItemQueryIterator<UnifiedWeatherResponse>(query);
                    if (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        var weatherData = response.FirstOrDefault();

                        if (weatherData != null && weatherData.HazardPrediction != null)
                        {
                            var prediction = weatherData.HazardPrediction;
                            if (prediction.FireRiskPercent > 0) hazards.Add("Fire");
                            if (prediction.FloodRiskPercent > 0) hazards.Add("Flood");
                            if (prediction.StormRiskPercent > 0) hazards.Add("Storm");
                            if (prediction.HeatWaveRiskPercent > 0) hazards.Add("Heat Wave");
                            if (prediction.SnowRiskPercent > 0) hazards.Add("Snow");

                            _logger.LogInformation("Fetched hazards for {City} from Cosmos DB", city);
                            return hazards;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hazards from Cosmos DB for {City}. Falling back to local files.", city);
            }

            // 2. Fallback to Local Files
            if (!Directory.Exists(_responsePath))
            {
                return hazards; // Return empty if path not found
            }

            // Find the latest file for the city
            // Pattern: {City}_{Date}.json
            var files = Directory.GetFiles(_responsePath, $"{city}_*.json");

            if (files.Length == 0)
            {
                return hazards;
            }

            // Sort by name (date) descending to get the latest
            var latestFile = files.OrderByDescending(f => f).First();

            try
            {
                var json = await File.ReadAllTextAsync(latestFile);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("hazard_prediction", out var prediction))
                {
                    // Check risks > 0 (or a threshold like 50)
                    if (prediction.TryGetProperty("fire_risk_percent", out var fire) && fire.GetInt32() > 0) hazards.Add("Fire");
                    if (prediction.TryGetProperty("flood_risk_percent", out var flood) && flood.GetInt32() > 0) hazards.Add("Flood");
                    if (prediction.TryGetProperty("storm_risk_percent", out var storm) && storm.GetInt32() > 0) hazards.Add("Storm");
                    if (prediction.TryGetProperty("heat_wave_risk_percent", out var heat) && heat.GetInt32() > 0) hazards.Add("Heat Wave");
                    if (prediction.TryGetProperty("snow_risk_percent", out var snow) && snow.GetInt32() > 0) hazards.Add("Snow");
                }
            }
            catch
            {
                // Log error or ignore
            }

            return hazards;
        }
    }
}
