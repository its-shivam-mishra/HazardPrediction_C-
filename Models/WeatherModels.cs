using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WeatherHazardApi.Models
{
    public class CityInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    // --- Open-Meteo Response Models ---
    public class OpenMeteoResponse
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public CurrentUnits? current_units { get; set; }
        public CurrentWeather? current { get; set; }
        public DailyUnits? daily_units { get; set; }
        public DailyWeather? daily { get; set; }
    }

    public class CurrentUnits
    {
        public string temperature_2m { get; set; } = "";
    }

    public class CurrentWeather
    {
        public string time { get; set; } = "";
        public int interval { get; set; }
        public double temperature_2m { get; set; }
        public int relative_humidity_2m { get; set; }
        public double apparent_temperature { get; set; }
        public int is_day { get; set; }
        public double precipitation { get; set; }
        public double rain { get; set; }
        public double showers { get; set; }
        public double snowfall { get; set; }
        public int weather_code { get; set; }
        public double cloud_cover { get; set; }
        public double pressure_msl { get; set; }
        public double surface_pressure { get; set; }
        public double wind_speed_10m { get; set; }
        public int wind_direction_10m { get; set; }
        public double wind_gusts_10m { get; set; }
    }

    public class DailyUnits
    {
        public string time { get; set; } = "";
        public string temperature_2m_max { get; set; } = "";
    }

    public class DailyWeather
    {
        public List<string> time { get; set; } = new();
        public List<double> temperature_2m_max { get; set; } = new();
        public List<double> temperature_2m_min { get; set; } = new();
        public List<double> precipitation_sum { get; set; } = new();
        public List<double> wind_speed_10m_max { get; set; } = new();
        public List<string> sunrise { get; set; } = new();
        public List<string> sunset { get; set; } = new();
        public List<int> weather_code { get; set; } = new();
        public List<double> uv_index_max { get; set; } = new();
    }

    // --- Unified Output Models ---
    public class UnifiedWeatherResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; } = new();

        [JsonProperty("weather")]
        public WeatherDetails Weather { get; set; } = new();

        [JsonProperty("HazardPrediction")]
        public HazardPrediction? HazardPrediction { get; set; }

        [JsonProperty("isNotificationSent")]
        public bool IsNotificationSent { get; set; }

        [JsonProperty("notificationSentDate")]
        public DateTime? NotificationSentDate { get; set; }

        [JsonProperty("predictionGeneratedDate")]
        public DateTime PredictionGeneratedDate { get; set; } = DateTime.UtcNow;

        [JsonProperty("Type")]
        public string Type { get; set; } = "Weather";
    }

    public class Coordinates
    {
        [JsonPropertyName("lat")]
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        [JsonProperty("lon")]
        public double Lon { get; set; }
    }

    public class WeatherDetails
    {
        [JsonPropertyName("temperature_c")]
        [JsonProperty("temperature_c")]
        public double TemperatureC { get; set; }

        [JsonPropertyName("feels_like_c")]
        [JsonProperty("feels_like_c")]
        public double FeelsLikeC { get; set; }

        [JsonPropertyName("humidity_percent")]
        [JsonProperty("humidity_percent")]
        public int HumidityPercent { get; set; }

        [JsonPropertyName("pressure_hpa")]
        [JsonProperty("pressure_hpa")]
        public int PressureHpa { get; set; }

        [JsonPropertyName("wind_speed_kph")]
        [JsonProperty("wind_speed_kph")]
        public double WindSpeedKph { get; set; }

        [JsonPropertyName("wind_direction_deg")]
        [JsonProperty("wind_direction_deg")]
        public int WindDirectionDeg { get; set; }

        [JsonPropertyName("visibility_km")]
        [JsonProperty("visibility_km")]
        public double VisibilityKm { get; set; }

        [JsonPropertyName("uv_index")]
        [JsonProperty("uv_index")]
        public int UvIndex { get; set; }

        [JsonPropertyName("dew_point_c")]
        [JsonProperty("dew_point_c")]
        public double DewPointC { get; set; }

        [JsonPropertyName("cloud_cover_percent")]
        [JsonProperty("cloud_cover_percent")]
        public int CloudCoverPercent { get; set; }

        [JsonPropertyName("precipitation_mm")]
        [JsonProperty("precipitation_mm")]
        public double PrecipitationMm { get; set; }

        [JsonPropertyName("snowfall_cm")]
        [JsonProperty("snowfall_cm")]
        public double SnowfallCm { get; set; }

        [JsonPropertyName("condition")]
        [JsonProperty("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("alerts")]
        [JsonProperty("alerts")]
        public List<string> Alerts { get; set; } = new();

        [JsonPropertyName("sun")]
        [JsonProperty("sun")]
        public SunInfo Sun { get; set; } = new();
    }

    public class SunInfo
    {
        [JsonPropertyName("sunrise")]
        [JsonProperty("sunrise")]
        public string Sunrise { get; set; } = string.Empty;

        [JsonPropertyName("sunset")]
        [JsonProperty("sunset")]
        public string Sunset { get; set; } = string.Empty;
    }

    // --- Hazard Prediction Models ---
    public class HazardPrediction
    {
        [JsonProperty("City")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("FireRiskPercent")]
        public int FireRiskPercent { get; set; }

        [JsonProperty("FloodRiskPercent")]
        public int FloodRiskPercent { get; set; }

        [JsonProperty("StormRiskPercent")]
        public int StormRiskPercent { get; set; }

        [JsonProperty("HeatWaveRiskPercent")]
        public int HeatWaveRiskPercent { get; set; }

        [JsonProperty("SnowRiskPercent")]
        public int SnowRiskPercent { get; set; }

        [JsonProperty("Explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
