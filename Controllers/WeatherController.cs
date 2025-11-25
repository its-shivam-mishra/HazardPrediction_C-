using Microsoft.AspNetCore.Mvc;
using WeatherHazardApi.Models;
using WeatherHazardApi.Services;

namespace WeatherHazardApi.Controllers
{
    [ApiController]
    [Route("weather")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IFileService _fileService;
        private readonly IHazardService _hazardService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(
            IWeatherService weatherService, 
            IFileService fileService, 
            IHazardService hazardService,
            ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _fileService = fileService;
            _hazardService = hazardService;
            _logger = logger;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchWeather([FromQuery] int days = 15)
        {
            _logger.LogInformation("Fetching weather for {Days} days", days);

            var weatherList = await _weatherService.FetchWeatherForAllCitiesAsync(days);
            var results = new List<UnifiedWeatherResponse>();

            foreach (var weather in weatherList)
            {
                // Predict hazards
                var hazard = await _hazardService.PredictHazardsAsync(weather);
                weather.HazardPrediction = hazard;

                // Save to file
                await _fileService.SaveWeatherAsync(weather);

                results.Add(weather);
            }

            return Ok(results);
        }
    }
}
