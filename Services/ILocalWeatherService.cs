using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface ILocalWeatherService
    {
        Task<(List<string> Hazards, string PredictionId)> GetHazardsForCityAsync(string city);
        Task<List<UnifiedWeatherResponse>> GetActiveHazardsAsync();
    }
}
