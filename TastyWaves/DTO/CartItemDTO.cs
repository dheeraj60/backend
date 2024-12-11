using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class CartItemDTO
    {
        [Required]
        public int MenuItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
