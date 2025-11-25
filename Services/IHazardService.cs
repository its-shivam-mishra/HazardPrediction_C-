using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface IHazardService
    {
        Task<HazardPrediction> PredictHazardsAsync(UnifiedWeatherResponse weatherData);
    }
}
