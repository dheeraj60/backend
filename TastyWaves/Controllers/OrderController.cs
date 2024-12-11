using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TastyWaves.Data;
using TastyWaves.Models;
using TastyWaves.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace TastyWaves.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensures that the controller requires authentication
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // Place a new order (Accessible by all roles)
        [HttpPost]
        [Authorize(Roles = "Customer,Restaurant,Admin")] // Accessible by customers, restaurant owners, and admins
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDTO newOrderDTO)
        {
            if (newOrderDTO == null)
            {
                return BadRequest("Order data is required.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing in the token.");
            }

            // Create a new Order entity from DTO
            var newOrder = new Order
            {
                UserId = userId,
                RestaurantId = newOrderDTO.RestaurantId,
                OrderDate = newOrderDTO.OrderDate,
                TotalAmount = newOrderDTO.TotalAmount,
                OrderStatus = newOrderDTO.OrderStatus ?? string.Empty, // Initialize to avoid null issues
                OrderItems = newOrderDTO.OrderItems.Select(oi => new OrderItem
                {
                    MenuItemId = oi.MenuItemId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderById), new { orderId = newOrder.OrderId }, newOrder);
        }

        // Get current user's order history (Accessible by customers and admins)
        [HttpGet("history")]
        [Authorize(Roles = "Customer,Admin")] // Accessible by customers and admins
        public async Task<IActionResult> GetOrderHistory()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID is missing in the token.");
            }

            var userId = int.Parse(userIdClaim);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found for this user.");
            }

            return Ok(orders);
        }

        // Get details of a specific order (Accessible by the order creator and admins)
        [HttpGet("{orderId}")]
        [Authorize(Roles = "Customer,Admin")] // Accessible by the order creator and admins
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId) && userId != order.UserId && User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Unauthorized("Access denied.");
            }

            return Ok(order);
        }

        // Update an order (Admin only)
        [HttpPut("{orderId}")]
        [Authorize(Roles = "Admin")] // Only admins can update an order
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] OrderDTO updatedOrderDTO)
        {
            if (updatedOrderDTO == null)
            {
                return BadRequest("Updated order data is required.");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Check if the user has the required role and permission
            if (User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Unauthorized("Access denied. Only admins can update an order.");
            }

            order.OrderStatus = updatedOrderDTO.OrderStatus ?? string.Empty; // Initialize to avoid null issues
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // Delete an order (Admin only)
        [HttpDelete("{orderId}")]
        [Authorize(Roles = "Admin")] // Only admins can delete an order
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok("Order deleted successfully.");
        }

        // Get all orders for a specific user (Admin only)
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")] // Only admins can access this method
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found for this user.");
            }

            return Ok(orders);
        }
    }
}
