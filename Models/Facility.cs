using System;

namespace SmartSocietyMVC.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }

        // Navigation Properties
        public int SocietyId { get; set; }
        public Society Society { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
