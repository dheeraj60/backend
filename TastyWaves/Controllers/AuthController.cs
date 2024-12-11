using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TastyWaves.Data;
using TastyWaves.Models;
using TastyWaves.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace TastyWaves.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO newUserDTO)
        {
            if (newUserDTO == null)
            {
                return BadRequest("User data is required.");
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newUserDTO.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

            // Create a new user and map DTO properties
            var newUser = new User
            {
                Name = newUserDTO.Name,
                Email = newUserDTO.Email,
                Password = newUserDTO.Password, // Implement password hashing in production
                Address = newUserDTO.Address,
                ContactNumber = newUserDTO.ContactNumber,
                Role = newUserDTO.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully.");
        }

        // Login and get a token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDetails)
        {
            if (loginDetails == null || string.IsNullOrEmpty(loginDetails.Email) || string.IsNullOrEmpty(loginDetails.Password))
            {
                return BadRequest("Invalid login data.");
            }

            // Validate user credentials
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDetails.Email && u.Password == loginDetails.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate and return JWT token
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        // Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
            var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
