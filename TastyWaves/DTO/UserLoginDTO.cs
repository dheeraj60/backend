using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;  // Non-nullable, initialized to avoid CS8618
    }
}
