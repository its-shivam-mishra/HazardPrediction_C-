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
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("coordinates")]
        public Coordinates Coordinates { get; set; } = new();

        [JsonPropertyName("weather")]
        public WeatherDetails Weather { get; set; } = new();
        
        [JsonPropertyName("hazard_prediction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HazardPrediction? HazardPrediction { get; set; }
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
        
        // Helper to hold daily forecast if needed, though the requirement asks for "Next N days" 
        // but the example output shows a single object. 
        // The requirement says "For each city, fetch: Next N days of weather"
        // BUT the example output structure shows a SINGLE "weather" object.
        // I will assume the example output is for "current" or "today", OR I should include a list of forecasts.
        // Re-reading: "Output JSON MUST follow this structure: ... weather: { ... }"
        // It looks like a single day snapshot in the example. 
        // However, "Fetch Next N days" implies a list.
        // I will add a `Forecast` property to be safe, or maybe the `weather` object is just the current/summary.
        // Let's stick to the example structure for the main object, and maybe add a `Forecasts` list if needed.
        // Actually, looking at the example, it has specific fields like `sunrise`, `sunset`.
        // I will strictly follow the example for the main structure, but I will add a `DailyForecasts` list 
        // to actually satisfy "Next N days of weather" requirement if the user wants to see them.
        // But to be strictly compliant with "Output JSON MUST follow this structure", I will match it exactly.
        // Maybe the "weather" object represents the *current* or *summary*?
        // I'll add a `Forecasts` list as an extra field, or just map the first day to the `weather` object.
        // Given the ambiguity, I'll map the current/today weather to the `weather` object as shown in the example,
        // and perhaps add a `forecast` array if it doesn't break the "MUST follow" rule.
        // The prompt says "Output JSON MUST follow this structure". I will stick to that structure.
        // If "Next N days" are fetched, maybe they are just used to determine hazards?
        // Or maybe the structure provided is just for one day and I should return a list of these?
        // "For each city, fetch... Output JSON MUST follow this structure".
        // It's likely a list of these objects, or one object per city?
        // "Create one file per city". So the file contains ONE JSON object like the example.
        // So the "weather" object probably represents the summary or the first day.
        // I will stick to the example structure.
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
