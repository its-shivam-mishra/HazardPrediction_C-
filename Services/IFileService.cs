using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface IFileService
    {
        Task SaveWeatherAsync(UnifiedWeatherResponse weatherData);
    }
}
