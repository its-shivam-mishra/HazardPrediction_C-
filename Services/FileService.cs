using System.Text.Json;
using Microsoft.Azure.Cosmos;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _cosmosClient = cosmosClient;
            _configuration = configuration;
        }

        public async Task SaveWeatherAsync(UnifiedWeatherResponse weatherData)
        {
            try
            {
                // 1. Save to Local File (Removed as per requirement)
                // Logic removed to rely solely on Cosmos DB

                // 2. Save to Cosmos DB
                // 2. Save to Cosmos DB
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(containerId))
                {
                    _logger.LogWarning("Cosmos DB configuration is missing. Skipping Cosmos DB save.");
                    return;
                }

                // Create Database if not exists
                Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

                // Create Container if not exists
                // Partition key path should be /city based on our usage: new PartitionKey(weatherData.City)

                Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/city");

                // Ensure Id is set if not already (though we added a default in the model)
                if (string.IsNullOrEmpty(weatherData.Id))
                {
                    weatherData.Id = Guid.NewGuid().ToString();
                }

                await container.UpsertItemAsync(weatherData, new PartitionKey(weatherData.City));

                _logger.LogInformation("Saved weather data to Cosmos DB for {City}", weatherData.City);

            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, "Cosmos DB Error saving weather data for {City}. Status: {Status}", weatherData.City, cosmosEx.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving weather data for {City}", weatherData.City);
            }
        }
    }
}
