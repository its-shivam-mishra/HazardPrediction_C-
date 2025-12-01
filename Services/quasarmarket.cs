using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherHazardApi.Services
{
    public class Quasarmarket
    {

        public async Task<string> llmResultAsync(string prompt, string endPoint, string key)
        {
            using var llm = new CustomLLM(
               key: key,
               endPoint: endPoint,
               model: "gpt-4o-mini", // Replace with your actual model name
               temperature: 0.5,
               topP: 0.9,
               maxTokens: 1000
            );

            string template = "You are a helpful assistant that outputs only JSON.: {text}";
            string userInput = PromptTemplate.Format(template, prompt);

            string response = await llm.InvokeAsync(userInput);

            return response; // Changed this to return the actual AI response instead of static string
        }
    }

    // 1. Define Request/Response objects to handle JSON serialization nicely
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    public class LlmRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public double TopP { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }
    }

    // 2. The CustomLLM Class
    public class CustomLLM : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpointUrl;

        // Properties
        public string Model { get; set; }
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 1.0;
        public int MaxTokens { get; set; } = 2000;

        public CustomLLM(string endPoint, string key, string model, double temperature = 0.2, double topP = 1.0, int maxTokens = 2000)
        {

            _endpointUrl = endPoint.TrimEnd('/');

            string apiKey = key;


            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            // Initialize HttpClient with the handler
            _httpClient = new HttpClient(handler);

            // Add Headers
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            // Set properties
            Model = model;
            Temperature = temperature;
            TopP = topP;
            MaxTokens = maxTokens;
        }

        public async Task<string> InvokeAsync(string prompt, List<string>? stop = null)
        {
            var payload = new LlmRequest
            {
                Model = this.Model,
                Messages = new List<ChatMessage> { new ChatMessage { Role = "user", Content = prompt } },
                Temperature = this.Temperature,
                TopP = this.TopP,
                MaxTokens = this.MaxTokens,
                Stop = stop
            };

            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload, jsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync(_endpointUrl, jsonContent);

                // This checks for success (200-299). 
                // Since we added AllowAutoRedirect=true, the client should have followed the 307 automatically.
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                // Parse the response
                using JsonDocument doc = JsonDocument.Parse(responseString);

                // Navigating data['choices'][0]['message']['content']
                string content = doc.RootElement
                                    .GetProperty("choices")[0]
                                    .GetProperty("message")
                                    .GetProperty("content")
                                    .GetString() ?? "";

                return content;
            }
            catch (Exception ex)
            {
                // Returns the error message so you can see what happened
                return $"Error calling LLM: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // 3. Simple Helper to mimic PromptTemplate
    public static class PromptTemplate
    {
        public static string Format(string template, string text)
        {
            return template.Replace("{text}", text);
        }
    }
}