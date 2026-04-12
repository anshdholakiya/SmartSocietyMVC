using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSocietyMVC.Models
{
    public class Society
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "Smart Society";

        public string? Address { get; set; }

        public string? ContactNumber { get; set; }

        // Storing JSON arrays as strings in EF Core
        public string Amenities { get; set; } = "[]";

        public string Wings { get; set; } = "[]";

        public string Gallery { get; set; } = "[]";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
        public ICollection<Notice> Notices { get; set; } = new List<Notice>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
