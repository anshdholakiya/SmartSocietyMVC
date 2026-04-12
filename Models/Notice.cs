using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Notice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Type { get; set; } = "alert"; // 'event', 'maintenance', 'alert'

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int SocietyId { get; set; }
        [ForeignKey("SocietyId")]
        public Society? Society { get; set; }
    }
}
