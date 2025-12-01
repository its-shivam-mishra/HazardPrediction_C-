using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("city")]
        [Newtonsoft.Json.JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("coordinates")]
        public Coordinates Coordinates { get; set; } = new();

        [JsonPropertyName("weather")]
        public WeatherDetails Weather { get; set; } = new();

        [JsonPropertyName("hazard_prediction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HazardPrediction? HazardPrediction { get; set; }

        [JsonPropertyName("isNotificationSent")]
        public bool IsNotificationSent { get; set; }

        [JsonPropertyName("notificationSentDate")]
        public DateTime? NotificationSentDate { get; set; }

        [JsonPropertyName("predictionGeneratedDate")]
        public DateTime PredictionGeneratedDate { get; set; } = DateTime.UtcNow;
    }

    public class Coordinates
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    public class WeatherDetails
    {
        [JsonPropertyName("temperature_c")]
        public double TemperatureC { get; set; }

        [JsonPropertyName("feels_like_c")]
        public double FeelsLikeC { get; set; }

        [JsonPropertyName("humidity_percent")]
        public int HumidityPercent { get; set; }

        [JsonPropertyName("pressure_hpa")]
        public int PressureHpa { get; set; }

        [JsonPropertyName("wind_speed_kph")]
        public double WindSpeedKph { get; set; }

        [JsonPropertyName("wind_direction_deg")]
        public int WindDirectionDeg { get; set; }

        [JsonPropertyName("visibility_km")]
        public double VisibilityKm { get; set; }

        [JsonPropertyName("uv_index")]
        public int UvIndex { get; set; }

        [JsonPropertyName("dew_point_c")]
        public double DewPointC { get; set; }

        [JsonPropertyName("cloud_cover_percent")]
        public int CloudCoverPercent { get; set; }

        [JsonPropertyName("precipitation_mm")]
        public double PrecipitationMm { get; set; }

        [JsonPropertyName("snowfall_cm")]
        public double SnowfallCm { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonPropertyName("alerts")]
        public List<string> Alerts { get; set; } = new();

        [JsonPropertyName("sun")]
        public SunInfo Sun { get; set; } = new();


    }

    public class SunInfo
    {
        [JsonPropertyName("sunrise")]
        public string Sunrise { get; set; } = string.Empty;

        [JsonPropertyName("sunset")]
        public string Sunset { get; set; } = string.Empty;
    }

    // --- Hazard Prediction Models ---
    public class HazardPrediction
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("fire_risk_percent")]
        public int FireRiskPercent { get; set; }

        [JsonPropertyName("flood_risk_percent")]
        public int FloodRiskPercent { get; set; }

        [JsonPropertyName("storm_risk_percent")]
        public int StormRiskPercent { get; set; }

        [JsonPropertyName("heat_wave_risk_percent")]
        public int HeatWaveRiskPercent { get; set; }

        [JsonPropertyName("snow_risk_percent")]
        public int SnowRiskPercent { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
