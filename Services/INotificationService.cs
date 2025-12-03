using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface INotificationService
    {
        Task<List<CityHazardGroup>> GetAtRiskUsersAsync();
        Task<CityHazardGroup?> GetCityHazardGroupForResendAsync(string predictionId, string city);
    }
}
