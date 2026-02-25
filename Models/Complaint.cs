using System;

namespace SmartSocietyMVC.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // pending, in-progress, resolved
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation Property
        public int UserId { get; set; }
        public User User { get; set; }
        public int SocietyId { get; set; }
        public Society Society { get; set; }
    }
}
