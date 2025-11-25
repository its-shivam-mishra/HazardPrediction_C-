using System.Text.Json;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task SaveWeatherAsync(UnifiedWeatherResponse weatherData)
        {
            try
            {
                // Ensure directory exists
                var folderPath = Path.Combine(_env.ContentRootPath, "response");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Filename: City_Date.json
                // Sanitize city name for filename
                var safeCityName = string.Join("", weatherData.City.Split(Path.GetInvalidFileNameChars()));
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                var fileName = $"{safeCityName}_{date}.json";
                var filePath = Path.Combine(folderPath, fileName);

                var json = JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
                
                _logger.LogInformation("Saved weather data for {City} to {Path}", weatherData.City, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving weather data for {City}", weatherData.City);
            }
        }
    }
}
