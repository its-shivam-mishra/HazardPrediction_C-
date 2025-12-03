using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUserService _userService;
        private readonly ILocalWeatherService _weatherService;

        public NotificationService(IUserService userService, ILocalWeatherService weatherService)
        {
            _userService = userService;
            _weatherService = weatherService;
        }

        public async Task<List<CityHazardGroup>> GetAtRiskUsersAsync()
        {
            var cityGroups = new List<CityHazardGroup>();

            // 1. Get all active hazards (grouped by city, latest only)
            var activeHazards = await _weatherService.GetActiveHazardsAsync();

            // 2. Get all users
            var users = await _userService.GetUsersAsync();

            // 3. Match users to hazards
            foreach (var weatherData in activeHazards)
            {
                if (weatherData.HazardPrediction == null) continue;

                var city = weatherData.City;
                var prediction = weatherData.HazardPrediction;
                var predictionId = weatherData.Id;

                // Identify hazards present in this prediction
                var currentHazards = new List<string>();
                if (prediction.FireRiskPercent > 0) currentHazards.Add("Fire");
                if (prediction.FloodRiskPercent > 0) currentHazards.Add("Flood");
                if (prediction.StormRiskPercent > 0) currentHazards.Add("Storm");
                if (prediction.HeatWaveRiskPercent > 0) currentHazards.Add("Heat Wave");
                if (prediction.SnowRiskPercent > 0) currentHazards.Add("Snow");

                if (!currentHazards.Any()) continue;

                // Find users in this city
                var usersInCity = users.Where(u => u.City.Equals(city, StringComparison.OrdinalIgnoreCase));
                var affectedUsers = new List<UserViewModel>();

                foreach (var user in usersInCity)
                {
                    var missingCoverages = new List<string>();

                    foreach (var hazard in currentHazards)
                    {
                        // Check if user has coverage for this hazard
                        bool hasCoverage = user.Coverages.Any(c => c.Name.Equals(hazard, StringComparison.OrdinalIgnoreCase));

                        if (!hasCoverage)
                        {
                            missingCoverages.Add(hazard);
                        }
                    }

                    if (missingCoverages.Any())
                    {
                        affectedUsers.Add(new UserViewModel
                        {
                            Id = user.Id,
                            Name = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            MissingCoverage = string.Join(", ", missingCoverages),
                            IsSelected = true,
                            PredictionId = predictionId,
                            City = user.City
                        });
                    }
                }

                if (affectedUsers.Any())
                {
                    cityGroups.Add(new CityHazardGroup
                    {
                        City = city,
                        PredictionId = predictionId,
                        HazardPrediction = prediction,
                        Users = affectedUsers,
                        IsSelected = true
                    });
                }
            }

            return cityGroups;
        }

        public async Task<CityHazardGroup?> GetCityHazardGroupForResendAsync(string predictionId, string city)
        {
            Console.WriteLine($"DEBUG: GetCityHazardGroupForResendAsync called. PredictionId: {predictionId}, City: {city}");
            // 1. Get the specific weather record (even if sent)
            var weatherData = await _weatherService.GetWeatherRecordAsync(predictionId, city);
            if (weatherData == null)
            {
                Console.WriteLine("DEBUG: WeatherData is null.");
                return null;
            }
            if (weatherData.HazardPrediction == null)
            {
                Console.WriteLine("DEBUG: HazardPrediction is null.");
                return null;
            }

            // 2. Get all users
            var users = await _userService.GetUsersAsync();
            Console.WriteLine($"DEBUG: Total users fetched: {users.Count}");

            var prediction = weatherData.HazardPrediction;

            // Identify hazards present in this prediction
            var currentHazards = new List<string>();
            if (prediction.FireRiskPercent > 0) currentHazards.Add("Fire");
            if (prediction.FloodRiskPercent > 0) currentHazards.Add("Flood");
            if (prediction.StormRiskPercent > 0) currentHazards.Add("Storm");
            if (prediction.HeatWaveRiskPercent > 0) currentHazards.Add("Heat Wave");
            if (prediction.SnowRiskPercent > 0) currentHazards.Add("Snow");

            Console.WriteLine($"DEBUG: Current Hazards: {string.Join(", ", currentHazards)}");

            if (!currentHazards.Any())
            {
                Console.WriteLine("DEBUG: No hazards found in prediction.");
                return null;
            }

            // Find users in this city
            var usersInCity = users.Where(u => u.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"DEBUG: Users in city {city}: {usersInCity.Count()}");
            var affectedUsers = new List<UserViewModel>();

            foreach (var user in usersInCity)
            {
                var missingCoverages = new List<string>();

                foreach (var hazard in currentHazards)
                {
                    // Check if user has coverage for this hazard
                    bool hasCoverage = user.Coverages.Any(c => c.Name.Equals(hazard, StringComparison.OrdinalIgnoreCase));

                    if (!hasCoverage)
                    {
                        missingCoverages.Add(hazard);
                    }
                }

                if (missingCoverages.Any())
                {
                    affectedUsers.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        MissingCoverage = string.Join(", ", missingCoverages),
                        IsSelected = true,
                        PredictionId = predictionId,
                        City = user.City
                    });
                }
            }

            Console.WriteLine($"DEBUG: Affected users found: {affectedUsers.Count}");

            if (affectedUsers.Any())
            {
                return new CityHazardGroup
                {
                    City = city,
                    PredictionId = predictionId,
                    HazardPrediction = prediction,
                    Users = affectedUsers,
                    IsSelected = true
                };
            }

            return null;
        }
    }
}
