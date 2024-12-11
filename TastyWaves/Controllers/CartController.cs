using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TastyWaves.Data;
using TastyWaves.Models;
using TastyWaves.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace TastyWaves.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensures that the controller requires authentication
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // View the current user's cart (Accessible by authenticated users)
        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetCart()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            return Ok(cart);
        }

        // Add an item to the cart (Accessible by authenticated users)
        [HttpPost("add")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO cartItemDTO)
        {
            if (cartItemDTO == null)
            {
                return BadRequest("Cart item data is required.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            // Find the user's cart or create a new one if it doesn't exist
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    TotalPrice = 0
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var menuItem = await _context.MenuItems.FindAsync(cartItemDTO.MenuItemId);
            if (menuItem == null)
            {
                return NotFound("Menu item not found.");
            }

            // Create a new CartItem and add it to the cart
            var cartItem = new CartItem
            {
                MenuItemId = cartItemDTO.MenuItemId,
                Quantity = cartItemDTO.Quantity,
                Price = cartItemDTO.Price,
                CartId = cart.CartId
            };

            _context.CartItems.Add(cartItem);
            cart.TotalPrice += cartItem.Price * cartItem.Quantity;
            await _context.SaveChangesAsync();

            return Ok("Item added to cart.");
        }

        // Update the quantity of an item in the cart (Accessible by authenticated users)
        [HttpPut("update/{cartItemId}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Ok(cartItem);
        }

        // Remove an item from the cart (Accessible by authenticated users)
        [HttpDelete("remove/{cartItemId}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok("Cart item removed.");
        }

        // Clear the cart (Accessible by authenticated users)
        [HttpDelete("clear")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> ClearCart()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.TotalPrice = 0;
            await _context.SaveChangesAsync();

            return Ok("Cart cleared.");
        }
    }
}
