using System;

namespace SmartSocietyMVC.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Month { get; set; } // Optional now
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; } // pending, paid
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation Property
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
