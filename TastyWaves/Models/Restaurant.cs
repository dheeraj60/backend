using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TastyWaves.Models
{
    public class Restaurant
    {
        [Key]
        public int RestaurantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Phone]
        public string? ContactNumber { get; set; }

        // Foreign key for User
        public int? UserId { get; set; } // Make it nullable

        // Navigation property to User
        public User? User { get; set; } // Make it nullable

        // Initialize navigation properties to avoid null warnings
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
