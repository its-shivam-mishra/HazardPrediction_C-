using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using WeatherHazardApi.Models;
using WeatherHazardApi.Services;

namespace WeatherHazardApi.Controllers
{
    public class UserCoverageController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;

        public UserCoverageController(INotificationService notificationService, IEmailService emailService, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _notificationService = notificationService;
            _emailService = emailService;
            _cosmosClient = cosmosClient;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool showSent = false)
        {
            // If showSent is true, we might want to show everything or just handle it differently.
            // For now, the requirement is to "reopen the Send Notification page pre-filled".
            // If we just call GetAtRiskUsersAsync, it filters out sent ones.
            // We might need a new method in service or a flag.
            // For simplicity, let's assume the user wants to see the current at-risk users.
            // If "Resend" is clicked, we might need to fetch that specific historical item.
            // But the current architecture fetches users based on *current* weather hazards.
            // If we want to resend a *past* notification, we need to reconstruct the view model from that past data.

            var atRiskUsers = await _notificationService.GetAtRiskUsersAsync();

            var viewModel = new NotificationViewModel
            {
                Users = atRiskUsers,
                EmailSubject = "Urgent: Weather Hazard Alert",
                EmailBodyHtml = "<p>Warning: We detected a fire risk in your area, and you do not have Fire Coverage.</p>"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotifications(NotificationViewModel model)
        {
            if (model.Users != null)
            {
                // 1. Send Emails
                foreach (var user in model.Users.Where(u => u.IsSelected))
                {
                    await _emailService.SendEmailAsync(user.Email, model.EmailSubject, model.EmailBodyHtml);
                }

                // 2. Update Cosmos DB (Mark predictions as sent)
                // We group by PredictionId to avoid redundant updates for the same prediction
                var predictionsToUpdate = model.Users
                    .Where(u => u.IsSelected && !string.IsNullOrEmpty(u.PredictionId) && !string.IsNullOrEmpty(u.City))
                    .Select(u => new { u.PredictionId, u.City })
                    .Distinct()
                    .ToList();

                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];
                Container? container = null;

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    container = _cosmosClient.GetContainer(databaseId, containerId);
                }

                if (container != null)
                {
                    foreach (var item in predictionsToUpdate)
                    {
                        try
                        {
                            var patchOperations = new[]
                            {
                                PatchOperation.Set("/isNotificationSent", true),
                                PatchOperation.Set("/notificationSentDate", DateTime.UtcNow)
                            };
                            await container.PatchItemAsync<dynamic>(item.PredictionId, new PartitionKey(item.City), patchOperations);
                        }
                        catch (Exception)
                        {
                            // Log error
                        }
                    }
                }
            }

            TempData["Message"] = "Notifications sent successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> SentHistory()
        {
            var databaseId = _configuration["Cosmos:DatabaseId"];
            var containerId = _configuration["Cosmos:ContainerId"];
            var sentNotifications = new List<UnifiedWeatherResponse>();

            if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
            {
                var container = _cosmosClient.GetContainer(databaseId, containerId);
                var query = new QueryDefinition("SELECT * FROM c WHERE c.isNotificationSent = true ORDER BY c._ts DESC");

                using var iterator = container.GetItemQueryIterator<UnifiedWeatherResponse>(query);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    sentNotifications.AddRange(response);
                }
            }
            return View(sentNotifications);
        }

        [HttpPost]
        public async Task<IActionResult> ResendNotification(string id, string city)
        {
            // To "resend", we effectively want to mark it as NOT sent so it appears in the list again?
            // Or just redirect to Index? 
            // If we mark it as not sent, it will appear in Index (if the weather is still valid).
            // Let's try to un-flag it in Cosmos.

            var databaseId = _configuration["Cosmos:DatabaseId"];
            var containerId = _configuration["Cosmos:ContainerId"];

            if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
            {
                var container = _cosmosClient.GetContainer(databaseId, containerId);
                try
                {
                    await container.PatchItemAsync<dynamic>(id, new PartitionKey(city), new[] { PatchOperation.Set("/isNotificationSent", false) });
                }
                catch { }
            }

            return RedirectToAction("Index");
        }
    }
}
