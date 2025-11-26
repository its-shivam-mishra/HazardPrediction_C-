using Azure.Communication.Email;

namespace WeatherHazardApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _connectionString;
        private readonly string _senderAddress;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _connectionString = configuration["EmailSettings:ConnectionString"] ?? string.Empty;
            _senderAddress = configuration["EmailSettings:SenderAddress"] ?? "DoNotReply@example.com";
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(_connectionString) || _connectionString == "YOUR_CONNECTION_STRING")
            {
                _logger.LogWarning($"Email simulation: To={toEmail}, Subject={subject}");
                return;
            }

            try
            {
                var emailClient = new EmailClient(_connectionString);
                var emailContent = new EmailContent(subject)
                {
                    Html = htmlBody
                };
                var emailMessage = new EmailMessage(_senderAddress, toEmail, emailContent);

                await emailClient.SendAsync(Azure.WaitUntil.Started, emailMessage);
                _logger.LogInformation($"Email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
            }
        }
    }
}
