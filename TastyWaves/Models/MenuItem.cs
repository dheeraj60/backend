using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TastyWaves.Models
{
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public bool Availability { get; set; }

        public string? SpecialDietaryInfo { get; set; }
        public string? TasteInfo { get; set; }
        public string? NutritionalInfo { get; set; }

        // Relationships
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
