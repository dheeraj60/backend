// OrderDTO.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TastyWaves.Models.DTOs
{
    public class OrderItemDTO
    {
        [Required]
        public int MenuItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }

    public class OrderDTO
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Make OrderStatus nullable to avoid warning, or initialize in constructor
        [Required]
        public string? OrderStatus { get; set; }

        [Required]
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
    }
}
