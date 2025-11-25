using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface IWeatherService
    {
        Task<List<UnifiedWeatherResponse>> FetchWeatherForAllCitiesAsync(int days);
    }
}
