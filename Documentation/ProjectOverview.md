# Weather Hazard Prediction API

## Project Architecture
This project is a .NET 8 Web API designed to fetch weather data, save it locally, and predict potential hazards using Azure AI Foundry (GPT models).

### Components
- **Controllers**: `WeatherController` handles HTTP requests.
- **Services**:
  - `WeatherService`: Fetches weather data from Open-Meteo API.
  - `FileService`: Saves weather data as JSON files in the `/response` directory.
  - `HazardService`: Uses Azure OpenAI to analyze weather data and predict hazards.
- **Models**: Defined in `Models/WeatherModels.cs` to structure API responses and internal data.

## Weather API Integration
The application integrates with [Open-Meteo](https://open-meteo.com/) to fetch forecast data.
- **Endpoint**: `https://api.open-meteo.com/v1/forecast`
- **Parameters**: Fetches temperature, humidity, wind speed, precipitation, etc., for the next N days.

## File Storage System
- Weather data is saved to the `/response` folder in the application root.
- **Filename Format**: `CityName_YYYY-MM-DD.json`
- The folder is automatically created if it does not exist.

## Azure AI Foundry Integration
The project uses `Azure.AI.OpenAI` to connect to GPT models.
- **Configuration**:
  - Endpoint and API Key are configured in `appsettings.json` or environment variables.
  - Uses `AzureOpenAIClient` to send weather data as a prompt.
- **Hazard Prediction**:
  - The model analyzes temperature, wind, humidity, etc.
  - Returns a JSON object with risk percentages for Fire, Flood, Storm, Heat Wave, and Snow.

## How to Run
1. **Prerequisites**: .NET 8 SDK.
2. **Configuration**:
   - Open `appsettings.json`.
   - Update `AzureOpenAI` section with your Endpoint and API Key.
3. **Build & Run**:
   ```bash
   dotnet build
   dotnet run
   ```
4. **Test API**:
   - Open Swagger UI at `http://localhost:5000/swagger` (or configured port).
   - Call `GET /weather/fetch?days=5`.

## Code Samples
### Azure OpenAI Client Initialization
```csharp
var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
```

### Hazard Prediction Call
```csharp
var chatClient = _client.GetChatClient(_deploymentName);
ChatCompletion completion = await chatClient.CompleteChatAsync(messages);
```
