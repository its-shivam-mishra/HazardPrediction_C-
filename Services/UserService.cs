using WeatherHazardApi.Models;

namespace WeatherHazardApi.Services
{
    public class UserService : IUserService
    {
        public Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    City = "Chicago",
                    State = "IL",
                    Coverages = new List<Coverage>
                    {
                        new Coverage { Id = 1, Name = "Health", Details = "Standard Health" }
                    }
                },
                new User
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    City = "New York",
                    State = "NY",
                    Coverages = new List<Coverage>
                    {
                        new Coverage { Id = 2, Name = "Vehicle", Details = "Full Coverage" },
                        new Coverage { Id = 3, Name = "Fire", Details = "Home Fire Insurance" }
                    }
                },
                 new User
                {
                    Id = 3,
                    FirstName = "Bob",
                    LastName = "Jones",
                    Email = "bob.jones@example.com",
                    City = "Chicago",
                    State = "IL",
                    Coverages = new List<Coverage>
                    {
                        new Coverage { Id = 2, Name = "Vehicle", Details = "Liability Only" }
                    }
                }
            };

            return Task.FromResult(users);
        }
    }
}
