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
    public class MenuItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuItemController(AppDbContext context)
        {
            _context = context;
        }

        // Get menu items for a specific restaurant (Accessible by Admins and Restaurant owners)
        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "Admin,Restaurant")]
        public async Task<IActionResult> GetMenuItemsByRestaurant(int restaurantId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId && r.UserId == userId);

            if (restaurant == null && User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Unauthorized("Access denied. Only admins or restaurant owners can access this data.");
            }

            var menuItems = await _context.MenuItems
                .Where(mi => mi.RestaurantId == restaurantId)
                .ToListAsync();

            if (menuItems == null || !menuItems.Any())
            {
                return NotFound("No menu items found for this restaurant.");
            }

            return Ok(menuItems);
        }

        // Get details of a specific menu item (Accessible by Admins and Restaurant owners)
        [HttpGet("{menuItemId}")]
        [Authorize(Roles = "Admin,Restaurant")]
        public async Task<IActionResult> GetMenuItemById(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound("Menu item not found.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && menuItem.RestaurantId != int.Parse(userIdClaim) && User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Unauthorized("Access denied.");
            }

            return Ok(menuItem);
        }

        // Add a new menu item (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMenuItem([FromBody] MenuItemDTO newMenuItemDTO)
        {
            if (newMenuItemDTO == null)
            {
                return BadRequest("Menu item data is required.");
            }

            var restaurantExists = await _context.Restaurants.AnyAsync(r => r.RestaurantId == newMenuItemDTO.RestaurantId);
            if (!restaurantExists)
            {
                return NotFound("Restaurant not found.");
            }

            var newMenuItem = new MenuItem
            {
                Name = newMenuItemDTO.Name,
                Description = newMenuItemDTO.Description,
                Category = newMenuItemDTO.Category,
                Price = newMenuItemDTO.Price,
                Availability = newMenuItemDTO.Availability,
                SpecialDietaryInfo = newMenuItemDTO.SpecialDietaryInfo,
                TasteInfo = newMenuItemDTO.TasteInfo,
                NutritionalInfo = newMenuItemDTO.NutritionalInfo,
                RestaurantId = newMenuItemDTO.RestaurantId
            };

            _context.MenuItems.Add(newMenuItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMenuItemById), new { menuItemId = newMenuItem.MenuItemId }, newMenuItem);
        }

        // Update a menu item (Admin only)
        [HttpPut("{menuItemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMenuItem(int menuItemId, [FromBody] MenuItemDTO updatedMenuItemDTO)
        {
            if (updatedMenuItemDTO == null)
            {
                return BadRequest("Updated menu item data is required.");
            }

            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound("Menu item not found.");
            }

            menuItem.Name = updatedMenuItemDTO.Name;
            menuItem.Description = updatedMenuItemDTO.Description;
            menuItem.Category = updatedMenuItemDTO.Category;
            menuItem.Price = updatedMenuItemDTO.Price;
            menuItem.Availability = updatedMenuItemDTO.Availability;
            menuItem.SpecialDietaryInfo = updatedMenuItemDTO.SpecialDietaryInfo;
            menuItem.TasteInfo = updatedMenuItemDTO.TasteInfo;
            menuItem.NutritionalInfo = updatedMenuItemDTO.NutritionalInfo;

            await _context.SaveChangesAsync();
            return Ok(menuItem);
        }

        // Delete a menu item (Admin only)
        [HttpDelete("{menuItemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItem(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound("Menu item not found.");
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return Ok("Menu item deleted successfully.");
        }
    }
}
