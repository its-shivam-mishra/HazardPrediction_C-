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
    },

    // --------------------------
    // NEW USERS START FROM HERE
    // --------------------------

    new User
    {
        Id = 4,
        FirstName = "Alice",
        LastName = "Walker",
        Email = "alice.walker@example.com",
        City = "Los Angeles",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 4, Name = "Health", Details = "Premium Health" }
        }
    },
    new User
    {
        Id = 5,
        FirstName = "Michael",
        LastName = "Brown",
        Email = "michael.brown@example.com",
        City = "Houston",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 5, Name = "Fire", Details = "Apartment Fire Insurance" }
        }
    },
    new User
    {
        Id = 6,
        FirstName = "Sarah",
        LastName = "Davis",
        Email = "sarah.davis@example.com",
        City = "Phoenix",
        State = "AZ",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 6, Name = "Vehicle", Details = "Standard Auto" }
        }
    },
    new User
    {
        Id = 7,
        FirstName = "David",
        LastName = "Miller",
        Email = "david.miller@example.com",
        City = "Philadelphia",
        State = "PA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 7, Name = "Health", Details = "Basic Health" },
            new Coverage { Id = 8, Name = "Vehicle", Details = "Full Auto Coverage" }
        }
    },
    new User
    {
        Id = 8,
        FirstName = "Emma",
        LastName = "Wilson",
        Email = "emma.wilson@example.com",
        City = "San Antonio",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 9, Name = "Fire", Details = "Fire + Natural Disaster" }
        }
    },
    new User
    {
        Id = 9,
        FirstName = "Chris",
        LastName = "Moore",
        Email = "chris.moore@example.com",
        City = "San Diego",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 10, Name = "Health", Details = "Gold Plan" }
        }
    },
    new User
    {
        Id = 10,
        FirstName = "Olivia",
        LastName = "Taylor",
        Email = "olivia.taylor@example.com",
        City = "Dallas",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 11, Name = "Vehicle", Details = "Collision + Liability" }
        }
    },
    new User
    {
        Id = 11,
        FirstName = "Liam",
        LastName = "Anderson",
        Email = "liam.anderson@example.com",
        City = "San Jose",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 12, Name = "Fire", Details = "Standard Fire Policy" }
        }
    },
    new User
    {
        Id = 12,
        FirstName = "Sophia",
        LastName = "Thomas",
        Email = "sophia.thomas@example.com",
        City = "New York",
        State = "NY",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 13, Name = "Health", Details = "Platinum Health" }
        }
    },
    new User
    {
        Id = 13,
        FirstName = "Benjamin",
        LastName = "Jackson",
        Email = "benjamin.jackson@example.com",
        City = "Phoenix",
        State = "AZ",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 14, Name = "Vehicle", Details = "Electric Car Insurance" }
        }
    },
    new User
    {
        Id = 14,
       FirstName = "Isabella",
        LastName = "White",
        Email = "isabella.white@example.com",
        City = "Chicago",
        State = "IL",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 15, Name = "Fire", Details = "Home Fire Coverage" }
        }
    },
    new User
    {
        Id = 15,
        FirstName = "Ethan",
        LastName = "Harris",
        Email = "ethan.harris@example.com",
        City = "Los Angeles",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 16, Name = "Health", Details = "Family Health Plan" }
        }
    },
    new User
    {
        Id = 16,
        FirstName = "Ava",
        LastName = "Martin",
        Email = "ava.martin@example.com",
        City = "Houston",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 17, Name = "Vehicle", Details = "Truck Insurance" }
        }
    },
    new User
    {
        Id = 17,
        FirstName = "James",
        LastName = "Garcia",
        Email = "james.garcia@example.com",
        City = "Philadelphia",
        State = "PA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 18, Name = "Fire", Details = "Warehouse Fire Policy" }
        }
    },
    new User
    {
        Id = 18,
        FirstName = "Mia",
        LastName = "Martinez",
        Email = "mia.martinez@example.com",
        City = "San Antonio",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 19, Name = "Health", Details = "Corporate Health" }
        }
    },
    new User
    {
        Id = 19,
        FirstName = "Alexander",
        LastName = "Robinson",
        Email = "alex.robinson@example.com",
        City = "San Diego",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 20, Name = "Vehicle", Details = "SUV Insurance" }
        }
    },
    new User
    {
        Id = 20,
        FirstName = "Charlotte",
        LastName = "Clark",
        Email = "charlotte.clark@example.com",
        City = "Dallas",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 21, Name = "Fire", Details = "Fire + Flood" }
        }
    },
    new User
    {
        Id = 21,
        FirstName = "Henry",
        LastName = "Rodriguez",
        Email = "henry.rodriguez@example.com",
        City = "San Jose",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 22, Name = "Health", Details = "Startup Employee Plan" }
        }
    },
    new User
    {
        Id = 22,
        FirstName = "Ella",
        LastName = "Lewis",
        Email = "ella.lewis@example.com",
        City = "Los Angeles",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 23, Name = "Vehicle", Details = "Sports Car Insurance" }
        }
    },
    new User
    {
        Id = 23,
        FirstName = "Daniel",
        LastName = "Lee",
        Email = "daniel.lee@example.com",
        City = "New York",
        State = "NY",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 24, Name = "Fire", Details = "Commercial Fire" }
        }
    },
    new User
    {
        Id = 24,
        FirstName = "Grace",
        LastName = "Walker",
        Email = "grace.walker@example.com",
        City = "Phoenix",
        State = "AZ",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 25, Name = "Health", Details = "Premium Maternity Plan" }
        }
    },
    new User
    {
        Id = 25,
        FirstName = "Jack",
        LastName = "Hall",
        Email = "jack.hall@example.com",
        City = "Houston",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 26, Name = "Vehicle", Details = "Hybrid Car Plan" }
        }
    },
    new User
    {
        Id = 26,
        FirstName = "Scarlett",
        LastName = "Young",
        Email = "scarlett.young@example.com",
        City = "San Diego",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 27, Name = "Fire", Details = "Wildfire Protection" }
        }
    },
    new User
    {
        Id = 27,
        FirstName = "Nathan",
        LastName = "Hernandez",
        Email = "nathan.hernandez@example.com",
        City = "San Antonio",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 28, Name = "Health", Details = "Basic Corporate Plan" }
        }
    },
    new User
    {
        Id = 28,
        FirstName = "Amelia",
        LastName = "King",
        Email = "amelia.king@example.com",
        City = "Dallas",
        State = "TX",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 29, Name = "Vehicle", Details = "Van Fleet Insurance" }
        }
    },
    new User
    {
        Id = 29,
        FirstName = "Logan",
        LastName = "Wright",
        Email = "logan.wright@example.com",
        City = "San Jose",
        State = "CA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 30, Name = "Health", Details = "Tech Employee Plus" }
        }
    },
    new User
    {
        Id = 30,
        FirstName = "Harper",
        LastName = "Lopez",
        Email = "harper.lopez@example.com",
        City = "Philadelphia",
        State = "PA",
        Coverages = new List<Coverage>
        {
            new Coverage { Id = 31, Name = "Fire", Details = "Full Home Protection" },
            new Coverage { Id = 32, Name = "Vehicle", Details = "Family Car Plan" }
        }
    }
};


            return Task.FromResult(users);
        }
    }
}
