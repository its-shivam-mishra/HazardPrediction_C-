namespace WeatherHazardApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public List<Coverage> Coverages { get; set; } = new();
    }

    public class Coverage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Fire", "Flood"
        public string Details { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MissingCoverage { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
    }

    public class NotificationViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBodyHtml { get; set; } = string.Empty;
    }
}
