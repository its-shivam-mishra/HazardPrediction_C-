using System.Text.Json;

namespace WeatherHazardApi.Services
{
    public class LocalWeatherService : ILocalWeatherService
    {
        private readonly string _responsePath;

        public LocalWeatherService(IWebHostEnvironment env)
        {
            _responsePath = Path.Combine(env.ContentRootPath, "response");
        }

        public async Task<List<string>> GetHazardsForCityAsync(string city)
        {
            var hazards = new List<string>();

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
