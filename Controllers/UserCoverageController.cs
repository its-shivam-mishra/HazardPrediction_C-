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
            var cityGroups = await _notificationService.GetAtRiskUsersAsync();

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
                            Console.WriteLine($"DEBUG: Log found. PredictionId: {log.PredictionId}, LogCity: {log.City}, UserIds Count: {log.UserIds?.Count}");

                            // Fetch the historical hazard group for this prediction
                            if (!string.IsNullOrEmpty(log.PredictionId))
                            {
                                var targetCities = (log.Cities != null && log.Cities.Any()) ? log.Cities : new List<string> { log.City };
                                cityGroups = new List<CityHazardGroup>();

                                foreach (var targetCity in targetCities)
                                {
                                    if (targetCity == "Multiple") continue;

                                    Console.WriteLine($"DEBUG: Processing Resend for City: {targetCity}");
                                    var resendGroup = await _notificationService.GetCityHazardGroupForResendAsync(log.PredictionId, targetCity);

                                    if (resendGroup != null)
                                    {
                                        Console.WriteLine($"DEBUG: ResendGroup found for {targetCity}. Users found: {resendGroup.Users.Count}");

                                        // Filter users within the group to match the original log
                                        var userIds = log.UserIds ?? new List<int>();
                                        resendGroup.Users = resendGroup.Users.Where(u => userIds.Contains(u.Id)).ToList();
                                        Console.WriteLine($"DEBUG: Users after filtering by log.UserIds: {resendGroup.Users.Count}");

                                        if (resendGroup.Users.Any())
                                        {
                                            cityGroups.Add(resendGroup);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"DEBUG: ResendGroup is NULL for {targetCity}.");
                                    }
                                }
                            }

                            // Ensure subject/body are from log
                            subject = log.EmailSubject;
                            body = log.EmailBodyHtml;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Error in Resend logic: {ex.Message}");
                    }
                }
            }

            var viewModel = new NotificationViewModel
            {
                CityGroups = cityGroups,
                EmailSubject = subject ?? "Urgent: Weather Hazard Alert",
                EmailBodyHtml = body ?? "Warning: We detected a risk in your area."
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotifications(NotificationViewModel model)
        {
            if (model.CityGroups != null && model.CityGroups.Any())
            {
                // Flatten to get selected users
                var selectedUsers = model.CityGroups.SelectMany(g => g.Users).Where(u => u.IsSelected).ToList();

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
                    
                    // Group selected users by PredictionId and City to ensure we only update records 
                    // where at least one user from that city was selected.
                    var distinctPredictions = selectedUsers
                        .Where(u => !string.IsNullOrEmpty(u.PredictionId) && !string.IsNullOrEmpty(u.City))
                        .GroupBy(u => new { u.PredictionId, u.City })
                        .Select(g => g.Key)
                        .ToList();

                    Console.WriteLine($"DEBUG: Updating {distinctPredictions.Count} weather records as sent.");

                    foreach (var item in distinctPredictions)
                    {
                        try
                        {
                            Console.WriteLine($"DEBUG: Marking weather record as sent for PredictionId: {item.PredictionId}, City: {item.City}");

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
                                Console.WriteLine($"DEBUG: Successfully updated weather record for {item.City}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating weather record {item.PredictionId} for city {item.City}: {ex.Message}");
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
