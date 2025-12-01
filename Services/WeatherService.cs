using System.Linq;
using System.Text.Json;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WeatherService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly List<CityInfo> _cities = new();

        public WeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<WeatherService> logger, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _env = env;

            var useMockCities = _configuration.GetValue<bool>("UseMockCities");
            if (useMockCities)
            {
                LoadMockCities();
            }
            else
            {
                LoadCitiesFromCsv();
            }
        }

        private void LoadMockCities()
        {
            _cities.AddRange(new[]
            {
                new CityInfo { Name = "New York", Lat = 40.7128, Lon = -74.0060 },
                new CityInfo { Name = "Los Angeles", Lat = 34.0522, Lon = -118.2437 },
                new CityInfo { Name = "Chicago", Lat = 41.8781, Lon = -87.6298 },
                new CityInfo { Name = "Houston", Lat = 29.7604, Lon = -95.3698 },
                new CityInfo { Name = "Phoenix", Lat = 33.4484, Lon = -112.0740 },
                new CityInfo { Name = "Philadelphia", Lat = 39.9526, Lon = -75.1652 },
                new CityInfo { Name = "San Antonio", Lat = 29.4241, Lon = -98.4936 },
                new CityInfo { Name = "San Diego", Lat = 32.7157, Lon = -117.1611 },
                new CityInfo { Name = "Dallas", Lat = 32.7767, Lon = -96.7970 },
                new CityInfo { Name = "San Jose", Lat = 37.3382, Lon = -121.8863 }
            });
            _logger.LogInformation("Loaded {Count} mock cities.", _cities.Count);
        }

        private void LoadCitiesFromCsv()
        {
            try
            {
                var filePath = Path.Combine(_env.WebRootPath, "data", "us_cities.csv");
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Cities CSV not found at {Path}. Using empty list.", filePath);
                    return;
                }

                var lines = File.ReadAllLines(filePath);
                // Skip header row
                foreach (var line in lines.Skip(1))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        if (double.TryParse(parts[2], out double lat) && double.TryParse(parts[3], out double lon))
                        {
                            _cities.Add(new CityInfo
                            {
                                Name = parts[0].Trim(),
                                Lat = lat,
                                Lon = lon
                            });
                        }
                    }
                }
                _logger.LogInformation("Loaded {Count} cities from CSV.", _cities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cities from CSV.");
            }
        }

        public async Task<List<UnifiedWeatherResponse>> FetchWeatherForAllCitiesAsync(int days)
        {
            var results = new List<UnifiedWeatherResponse>();
            var baseUrl = _configuration["OpenMeteo:BaseUrl"] ?? "https://api.open-meteo.com/v1/forecast";

            foreach (var city in _cities)
            {
                try
                {
                    var url = $"{baseUrl}?latitude={city.Lat}&longitude={city.Lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,cloud_cover,precipitation,snowfall,weather_code&daily=temperature_2m_max,temperature_2m_min,sunrise,sunset,uv_index_max,precipitation_sum,wind_speed_10m_max&forecast_days={days}&timezone=auto";

                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var weatherData = JsonSerializer.Deserialize<OpenMeteoResponse>(json);

                    if (weatherData != null)
                    {
                        results.Add(MapToUnified(city, weatherData));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching weather for {City}", city.Name);
                }
            }

            return results;
        }

        private UnifiedWeatherResponse MapToUnified(CityInfo city, OpenMeteoResponse data)
        {
            var current = data.current;
            var daily = data.daily;

            // Helper to get condition string from WMO code
            string GetCondition(int code) => code switch
            {
                0 => "Clear sky",
                1 => "Mainly clear",
                2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Fog",
                51 or 53 or 55 => "Drizzle",
                61 or 63 or 65 => "Rain",
                71 or 73 or 75 => "Snow",
                95 or 96 or 99 => "Thunderstorm",
                _ => "Unknown"
            };

            var response = new UnifiedWeatherResponse
            {
                City = city.Name,
                Coordinates = new Coordinates
                {
                    Lat = city.Lat,
                    Lon = city.Lon
                },
                Weather = new WeatherDetails
                {
                    TemperatureC = current?.temperature_2m ?? 0,
                    FeelsLikeC = current?.apparent_temperature ?? 0,
                    HumidityPercent = current?.relative_humidity_2m ?? 0,
                    PressureHpa = (int)(current?.surface_pressure ?? 0),
                    WindSpeedKph = current?.wind_speed_10m ?? 0,
                    WindDirectionDeg = current?.wind_direction_10m ?? 0,
                    VisibilityKm = 10,
                    UvIndex = (int)(daily?.uv_index_max?.FirstOrDefault() ?? 0),
                    DewPointC = 0, // Not requested
                    CloudCoverPercent = (int)(current?.cloud_cover ?? 0),
                    PrecipitationMm = current?.precipitation ?? 0,
                    SnowfallCm = current?.snowfall ?? 0,
                    Condition = GetCondition(current?.weather_code ?? -1),
                    Alerts = new List<string>(),
                    Sun = new SunInfo
                    {
                        Sunrise = daily?.sunrise?.FirstOrDefault() ?? "",
                        Sunset = daily?.sunset?.FirstOrDefault() ?? ""
                    }
                }
            };

            return response;
        }
    }
}
