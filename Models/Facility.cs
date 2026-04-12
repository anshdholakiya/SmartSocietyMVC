using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Facility
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? Capacity { get; set; }

        public string Status { get; set; } = "Available"; // Available, Maintenance, Closed

        public string? OperatingHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; } = 0.00m;

        [Required]
        public int SocietyId { get; set; }
        [ForeignKey("SocietyId")]
        public Society? Society { get; set; }

        // Navigation properties
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
