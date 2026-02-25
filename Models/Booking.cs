using System;

namespace SmartSocietyMVC.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Days { get; set; }
        public decimal TotalPrice { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; } // pending, approved, rejected
        
        // Navigation Properties
        public int FacilityId { get; set; }
        public Facility Facility { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
