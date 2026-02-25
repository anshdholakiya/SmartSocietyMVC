using System;

namespace SmartSocietyMVC.Models
{
    public class Notice
    {
        public int Id { get; set; }
        public string Type { get; set; } // alert, event, maintenance
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public int SocietyId { get; set; }
        public Society Society { get; set; }
    }
}
