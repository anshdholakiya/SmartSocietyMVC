using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;
        
        public string Organizer { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int SocietyId { get; set; }
        
        [ForeignKey("SocietyId")]
        public Society? Society { get; set; }
    }
}
