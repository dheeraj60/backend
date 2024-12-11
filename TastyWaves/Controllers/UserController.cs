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
    [Authorize] // Ensure that only authenticated users can access this controller
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // Get all user details (Admin only)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // Only admins can access this method
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }

            var userDTOs = users.Select(u => new UserProfileDTO
            {
                Name = u.Name,
                Email = u.Email,
                Address = u.Address,
                ContactNumber = u.ContactNumber
            }).ToList();

            return Ok(userDTOs);
        }

        // Get details of a specific user by ID (Admin only)
        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin")] // Only admins can access this method
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDTO = new UserProfileDTO
            {
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                ContactNumber = user.ContactNumber
            };

            return Ok(userDTO);
        }

        // Get current user profile (Accessible by all roles)
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID is missing in the token.");
            }

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDTO = new UserProfileDTO
            {
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                ContactNumber = user.ContactNumber
            };

            return Ok(userDTO);
        }

        // Update user profile (Accessible by the user themselves and Admins)
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDTO updatedUserDTO)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID is missing in the token.");
            }

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Allow updates only if the user is themselves or an Admin
            if (User.FindFirstValue(ClaimTypes.Role) != "Admin" && user.UserId != int.Parse(userIdClaim))
            {
                return Unauthorized("Access denied. You can only update your own profile.");
            }

            // Update fields only if they are provided in the request
            if (updatedUserDTO.Name != null) user.Name = updatedUserDTO.Name;
            if (updatedUserDTO.Email != null) user.Email = updatedUserDTO.Email;
            if (updatedUserDTO.Address != null) user.Address = updatedUserDTO.Address;
            if (updatedUserDTO.ContactNumber != null) user.ContactNumber = updatedUserDTO.ContactNumber;

            await _context.SaveChangesAsync();

            var userDTO = new UserProfileDTO
            {
                Name = user.Name,
                Email = user.Email,
                Address = user.Address,
                ContactNumber = user.ContactNumber
            };

            return Ok(userDTO);
        }

        // Delete a user (Admin only)
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")] // Only admins can access this method
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted successfully.");
        }

        // Additional endpoint examples for specific roles

        // Get orders of the current user (Accessible by Customers and Admins)
        [HttpGet("orders")]
        [Authorize(Roles = "Customer,Admin")] // Accessible by customers and admins
        public async Task<IActionResult> GetUserOrders()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID is missing in the token.");
            }

            var userId = int.Parse(userIdClaim);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found for this user.");
            }

            return Ok(orders);
        }

        // Restaurant-specific access (e.g., viewing restaurant's own data)
        [HttpGet("my-restaurant")]
        [Authorize(Roles = "Restaurant,Admin")] // Accessible by restaurant owners and admins
        public async Task<IActionResult> GetMyRestaurantData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID is missing in the token.");
            }

            var userId = int.Parse(userIdClaim);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.UserId == userId);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found for this user.");
            }

            return Ok(restaurant);
        }
    }
}
