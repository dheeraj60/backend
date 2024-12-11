using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        [MaxLength(200)]
        public string? Address { get; set; }  // Nullable, can be null

        [Phone]
        public string? ContactNumber { get; set; }  // Nullable, can be null

        [Required]
        public string Role { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618
    }
}
