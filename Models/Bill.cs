using System;

namespace SmartSocietyMVC.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public string Month { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // pending, paid
        
        // Navigation Property
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
