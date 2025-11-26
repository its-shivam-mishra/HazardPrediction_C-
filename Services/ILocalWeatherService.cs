namespace WeatherHazardApi.Services
{
    public interface ILocalWeatherService
    {
        Task<List<string>> GetHazardsForCityAsync(string city);
    }
}
