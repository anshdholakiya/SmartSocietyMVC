using System;

namespace SmartSocietyMVC.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Added for real auth
        public string Role { get; set; } // e.g., admin, resident
        public string? Wing { get; set; } // Nullable because Admin might not have one initially
        public string? FlatNumber { get; set; }
        public bool IsSetup { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public int? SocietyId { get; set; }
        public Society Society { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
        public ICollection<Bill> Bills { get; set; }
    }
}
