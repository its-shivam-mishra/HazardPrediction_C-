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
        public async Task<IActionResult> Index(string? subject = null, string? body = null, string? notificationId = null, string? city = null)
        {
            var atRiskUsers = await _notificationService.GetAtRiskUsersAsync();

            // If Resend (notificationId provided), filter users to the original list
            if (!string.IsNullOrEmpty(notificationId) && !string.IsNullOrEmpty(city))
            {
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];
                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    try
                    {
                        var container = _cosmosClient.GetContainer(databaseId, containerId);
                        ItemResponse<NotificationLog> response = await container.ReadItemAsync<NotificationLog>(notificationId, new PartitionKey(city));
                        var log = response.Resource;

                        if (log != null)
                        {
                            // Filter users
                            atRiskUsers = atRiskUsers.Where(u => log.UserIds.Contains(u.Id)).ToList();

                            // Ensure subject/body are from log (though params should handle it)
                            subject = log.EmailSubject;
                            body = log.EmailBodyHtml;
                        }
                    }
                    catch { /* Log not found or error, show all users */ }
                }
            }

            var viewModel = new NotificationViewModel
            {
                Users = atRiskUsers,
                EmailSubject = subject ?? "Urgent: Weather Hazard Alert",
                EmailBodyHtml = body ?? "<p>Warning: We detected a fire risk in your area, and you do not have Fire Coverage.</p>"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotifications(NotificationViewModel model)
        {
            if (model.Users != null)
            {
                var selectedUsers = model.Users.Where(u => u.IsSelected).ToList();

                // 1. Send Emails
                foreach (var user in selectedUsers)
                {
                    await _emailService.SendEmailAsync(user.Email, model.EmailSubject, model.EmailBodyHtml);
                }

                // 2. Save to Notification Container (Shared)
                var databaseId = _configuration["Cosmos:DatabaseId"];
                var containerId = _configuration["Cosmos:ContainerId"];

                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    // Create a single log for the batch
                    var distinctCities = selectedUsers.Select(u => u.City).Distinct().ToList();
                    var partitionKeyCity = distinctCities.Count == 1 ? distinctCities.First() : "Multiple";
                    var predictionId = selectedUsers.FirstOrDefault()?.PredictionId ?? string.Empty;

                    var log = new NotificationLog
                    {
                        UserIds = selectedUsers.Select(u => u.Id).ToList(),
                        CoverageIds = selectedUsers.Select(u => u.MissingCoverage).Distinct().ToList(),
                        EmailSubject = model.EmailSubject,
                        EmailBodyHtml = model.EmailBodyHtml,
                        SentDate = DateTime.UtcNow,
                        City = partitionKeyCity,
                        Cities = distinctCities,
                        PredictionId = predictionId,
                        Type = "Notification"
                    };

                    try
                    {
                        var container = _cosmosClient.GetContainer(databaseId, containerId);
                        await container.CreateItemAsync(log, new PartitionKey(partitionKeyCity));
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        Console.WriteLine(ex.Message);
                    }
                }

                // 3. Update Weather Records (Mark as Sent)
                if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);
                    var distinctPredictions = selectedUsers
                        .Where(u => !string.IsNullOrEmpty(u.PredictionId) && !string.IsNullOrEmpty(u.City))
                        .GroupBy(u => new { u.PredictionId, u.City })
                        .Select(g => g.Key)
                        .ToList();

                    foreach (var item in distinctPredictions)
                    {
                        try
                        {
                            // Fetch the weather record
                            ItemResponse<UnifiedWeatherResponse> response = await container.ReadItemAsync<UnifiedWeatherResponse>(item.PredictionId, new PartitionKey(item.City));
                            var weatherRecord = response.Resource;

                            if (weatherRecord != null)
                            {
                                // Update fields
                                weatherRecord.IsNotificationSent = true;
                                weatherRecord.NotificationSentDate = DateTime.UtcNow;

                                // Save back
                                await container.UpsertItemAsync(weatherRecord, new PartitionKey(item.City));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating weather record {item.PredictionId}: {ex.Message}");
                        }
                    }
                }
            }

            TempData["Message"] = "Notifications sent successfully!";
            // return RedirectToAction("Index");

            return RedirectToAction("SentHistory");
        }

        [HttpGet]
        public async Task<IActionResult> SentHistory()
        {
            var databaseId = _configuration["Cosmos:DatabaseId"];
            var containerId = _configuration["Cosmos:ContainerId"];
            var logs = new List<NotificationLog>();

            if (!string.IsNullOrEmpty(databaseId) && !string.IsNullOrEmpty(containerId))
            {
                try
                {
                    var container = _cosmosClient.GetContainer(databaseId, containerId);
                    // Filter by Type = 'Notification'
                    var query = new QueryDefinition("SELECT * FROM c WHERE c.Type = 'Notification' ORDER BY c.sentDate DESC");

                    using var iterator = container.GetItemQueryIterator<NotificationLog>(query);
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        logs.AddRange(response);
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Container might not exist yet
                }
            }
            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> ResendNotification(string id, string city)
        {
            // Redirect to Index with notificationId to trigger filtering
            return RedirectToAction("Index", new { notificationId = id, city = city });
        }
    }
}
