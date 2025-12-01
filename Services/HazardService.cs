using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class HazardService : IHazardService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HazardService> _logger;
        private readonly ChatClient? _chatClient;
        private readonly string _deploymentName;
        private readonly string _quasarEndpoint;
        private readonly string _quasarKey;

        public HazardService(IConfiguration configuration, ILogger<HazardService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var key = _configuration["AzureOpenAI:ApiKey"];
            _quasarEndpoint = _configuration["Quasarmarket:Endpoint"]??"";
            _quasarKey = _configuration["Quasarmarket:ApiKey"]??"";
            _deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

            if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(key))
            {
                try
                {
                    var options = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_12_01_Preview);
                    try
                    {
                        var versions = Enum.GetNames(typeof(AzureOpenAIClientOptions.ServiceVersion));
                        _logger.LogInformation($"Available ServiceVersions: {string.Join(", ", versions)}");
                    }
                    catch (Exception e) { _logger.LogError(e, "Could not list versions"); }

                    var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key), options);
                    _chatClient = azureClient.GetChatClient(_deploymentName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize AzureOpenAIClient");
                }
            }
            else
            {
                _logger.LogWarning("Azure OpenAI credentials not found. Hazard prediction will return mock data.");
            }
        }

        public async Task<HazardPrediction> PredictHazardsAsync(UnifiedWeatherResponse weatherData)
        {
            if (_chatClient == null)
            {
                return GetMockPrediction(weatherData.City);
            }

            try
            {
                var weatherJson = JsonSerializer.Serialize(weatherData);
                var prompt = $@"
                                You are a weather hazard expert. Analyze the following weather data for {weatherData.City} and predict the risk percentage (0-100) for various hazards.
                                Return ONLY a valid JSON object with the following structure, no markdown formatting:
                                {{
                                  ""city"": ""{weatherData.City}"",
                                  ""fire_risk_percent"": 0,
                                  ""flood_risk_percent"": 0,
                                  ""storm_risk_percent"": 0,
                                  ""heat_wave_risk_percent"": 0,
                                  ""snow_risk_percent"": 0
                                }}

                                Weather Data:
                                {weatherJson}
                                ";

                //ChatCompletion completion = await _chatClient.CompleteChatAsync(
                //     [
                //         new SystemChatMessage("You are a helpful assistant that outputs only JSON."),
                //         new UserChatMessage(prompt),
                //     ]);

                // var responseText = completion.Content[0].Text;
                var responseText = await new Quasarmarket().llmResultAsync(prompt,_quasarEndpoint,_quasarKey);
                // Clean up markdown code blocks if present (just in case)
                responseText = responseText.Replace("```json", "").Replace("```", "").Trim();

                var prediction = JsonSerializer.Deserialize<HazardPrediction>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return prediction ?? GetMockPrediction(weatherData.City);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Azure OpenAI for hazard prediction");
                return GetMockPrediction(weatherData.City);
            }
        }

        private HazardPrediction GetMockPrediction(string city)
        {
            return new HazardPrediction
            {
                City = city,
                FireRiskPercent = 5,
                FloodRiskPercent = 5,
                StormRiskPercent = 5,
                HeatWaveRiskPercent = 5,
                SnowRiskPercent = 0,
                Explanation = "Azure OpenAI not configured or failed. Returning mock data."
            };
        }
    }
}
