using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int FacilityId { get; set; }
        [ForeignKey("FacilityId")]
        public Facility? Facility { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Status { get; set; } = "pending"; // 'pending', 'approved', 'rejected'

        public string? Purpose { get; set; }

        public string? RejectReason { get; set; }

        public int Days { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0.00m;
    }
}
