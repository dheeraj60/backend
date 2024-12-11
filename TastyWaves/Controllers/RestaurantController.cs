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
    public class RestaurantController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantController(AppDbContext context)
        {
            _context = context;
        }

        // Get all restaurants (Accessible by Admins and Restaurant Owners)
        [HttpGet]
        [Authorize(Roles = "Admin,Restaurant")]
        public async Task<IActionResult> GetAllRestaurants()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            if (userRole == "Admin")
            {
                var restaurants = await _context.Restaurants.ToListAsync();
                if (restaurants == null || !restaurants.Any())
                {
                    return NotFound("No restaurants found.");
                }
                return Ok(restaurants);
            }
            else if (userRole == "Restaurant")
            {
                var restaurants = await _context.Restaurants
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                if (restaurants == null || !restaurants.Any())
                {
                    return NotFound("No restaurants found for this user.");
                }
                return Ok(restaurants);
            }
            else
            {
                return Unauthorized("Access denied.");
            }
        }

        // Get details of a specific restaurant (Accessible by Admins and the restaurant owner)
        [HttpGet("{restaurantId}")]
        [Authorize(Roles = "Admin,Restaurant")]
        public async Task<IActionResult> GetRestaurantById(int restaurantId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID is missing or invalid.");
            }

            var restaurant = await _context.Restaurants
                .Include(r => r.User) // Include User for checking ownership if needed
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found.");
            }

            // Check if the user is allowed to access this restaurant
            if (User.FindFirstValue(ClaimTypes.Role) == "Restaurant" && restaurant.UserId != userId)
            {
                return Unauthorized("Access denied. You can only view your own restaurant data.");
            }

            return Ok(restaurant);
        }

        // Add a new restaurant (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRestaurant([FromBody] RestaurantDTO newRestaurantDTO)
        {
            if (newRestaurantDTO == null)
            {
                return BadRequest("Restaurant data is required.");
            }

            // Convert DTO to Restaurant entity
            var newRestaurant = new Restaurant
            {
                Name = newRestaurantDTO.Name,
                Location = newRestaurantDTO.Location,
                ContactNumber = newRestaurantDTO.ContactNumber,
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : (int?)null
            };

            _context.Restaurants.Add(newRestaurant);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRestaurantById), new { restaurantId = newRestaurant.RestaurantId }, newRestaurant);
        }

        // Update a restaurant (Admin only)
        [HttpPut("{restaurantId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRestaurant(int restaurantId, [FromBody] RestaurantDTO updatedRestaurantDTO)
        {
            if (updatedRestaurantDTO == null)
            {
                return BadRequest("Updated restaurant data is required.");
            }

            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurant not found.");
            }

            // Check if the user has the required role and owns the restaurant
            if (User.FindFirstValue(ClaimTypes.Role) != "Admin")
            {
                return Unauthorized("Access denied. Only admins can update a restaurant.");
            }

            // Update properties from the DTO
            restaurant.Name = updatedRestaurantDTO.Name;
            restaurant.Location = updatedRestaurantDTO.Location;
            restaurant.ContactNumber = updatedRestaurantDTO.ContactNumber;

            await _context.SaveChangesAsync();
            return Ok(new RestaurantDTO
            {
                Name = restaurant.Name,
                Location = restaurant.Location,
                ContactNumber = restaurant.ContactNumber
            });
        }

        // Delete a restaurant (Admin only)
        [HttpDelete("{restaurantId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRestaurant(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurant not found.");
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            return Ok("Restaurant deleted successfully.");
        }
    }
}
