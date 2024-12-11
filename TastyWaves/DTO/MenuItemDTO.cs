using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class MenuItemDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public bool Availability { get; set; }

        [MaxLength(100)]
        public string? SpecialDietaryInfo { get; set; }

        [MaxLength(100)]
        public string? TasteInfo { get; set; }

        [MaxLength(100)]
        public string? NutritionalInfo { get; set; }

        [Required]
        public int RestaurantId { get; set; }
    }
}
