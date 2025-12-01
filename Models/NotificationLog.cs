using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WeatherHazardApi.Models
{
    public class NotificationLog
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("userIds")]
        [JsonProperty("userIds")]
        public List<int> UserIds { get; set; } = new();

        [JsonPropertyName("coverageIds")]
        [JsonProperty("coverageIds")]
        public List<string> CoverageIds { get; set; } = new(); // Storing missing coverage names/details

        [JsonPropertyName("emailSubject")]
        [JsonProperty("emailSubject")]
        public string EmailSubject { get; set; } = string.Empty;

        [JsonPropertyName("emailBodyHtml")]
        [JsonProperty("emailBodyHtml")]
        public string EmailBodyHtml { get; set; } = string.Empty;

        [JsonPropertyName("sentDate")]
        [JsonProperty("sentDate")]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("city")]
        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("cities")]
        [JsonProperty("cities")]
        public List<string> Cities { get; set; } = new();

        [JsonPropertyName("predictionId")]
        [JsonProperty("predictionId")]
        public string PredictionId { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        [JsonProperty("Type")]
        public string Type { get; set; } = "Notification";
    }
}
