using System;
using System.Collections.Generic;

namespace SmartSocietyMVC.Models
{
    public class Society
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public ICollection<User> Users { get; set; }
        public ICollection<Facility> Facilities { get; set; }
        public ICollection<Notice> Notices { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
