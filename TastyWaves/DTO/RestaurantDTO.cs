using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class RestaurantDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Phone]
        public string? ContactNumber { get; set; }
    }
}
