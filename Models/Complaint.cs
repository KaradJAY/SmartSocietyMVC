using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSocietyMVC.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string Status { get; set; } = "pending"; // 'pending', 'resolved', 'in-progress'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int SocietyId { get; set; }
        [ForeignKey("SocietyId")]
        public Society? Society { get; set; }
    }
}
