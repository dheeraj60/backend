using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class UpdateUserDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
    }
}
