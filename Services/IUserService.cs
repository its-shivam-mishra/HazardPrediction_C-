using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
    }
}
