using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface INotificationService
    {
        Task<List<UserViewModel>> GetAtRiskUsersAsync();
    }
}
