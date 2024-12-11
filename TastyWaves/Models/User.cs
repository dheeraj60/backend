using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TastyWaves.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        // Initialize navigation properties to avoid null warnings
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Cart? Cart { get; set; }
        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>(); // Add if needed for restaurant ownership
    }
}
