using Microsoft.AspNetCore.Mvc;
using WeatherHazardApi.Models;
using WeatherHazardApi.Services;

namespace WeatherHazardApi.Controllers
{
    public class UserCoverageController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public UserCoverageController(INotificationService notificationService, IEmailService emailService)
        {
            _notificationService = notificationService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
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
                foreach (var user in model.Users.Where(u => u.IsSelected))
                {
                    await _emailService.SendEmailAsync(user.Email, model.EmailSubject, model.EmailBodyHtml);
                }
            }

            TempData["Message"] = "Notifications sent successfully!";
            return RedirectToAction("Index");
        }
    }
}
