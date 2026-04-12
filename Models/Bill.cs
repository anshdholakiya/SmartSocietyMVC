using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Type { get; set; } = "maintenance"; // 'maintenance', 'event', 'penalty', 'other'

        public string? Description { get; set; }

        public string? Month { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Penalty { get; set; } = 0.00m;

        public string Status { get; set; } = "pending"; // 'pending', 'paid', 'overdue'
    }
}
