using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "resident";

        public string? Wing { get; set; }
        public string? FlatNumber { get; set; }
        public string? ParkingSlot { get; set; }
        public string? Profession { get; set; }
        public string? ProfilePicture { get; set; }
        public string? ProfilePictureId { get; set; }

        public string? InvitationToken { get; set; }
        public bool IsSetup { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int SocietyId { get; set; }
        [ForeignKey("SocietyId")]
        public Society? Society { get; set; }

        // Navigation properties
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
