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

        public async Task<List<UserViewModel>> GetAtRiskUsersAsync()
        {
            var users = await _userService.GetUsersAsync();
            var atRiskUsers = new List<UserViewModel>();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.City)) continue;

                var (hazards, predictionId) = await _weatherService.GetHazardsForCityAsync(user.City);
                var missingCoverages = new List<string>();

                foreach (var hazard in hazards)
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
                    atRiskUsers.Add(new UserViewModel
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

            return atRiskUsers;
        }
    }
}
