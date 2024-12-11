using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class UserProfileDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        public string? Address { get; set; }  // Nullable, can be null
        public string? ContactNumber { get; set; }  // Nullable, can be null
    }
}
