using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using TastyWaves.Controllers;
using TastyWaves.Data;
using TastyWaves.Models;
using TastyWaves.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyWaves.Tests.Controllers
{
    [TestFixture]
    public class ControllerTests
    {
        private AppDbContext _context;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);

            var configurationData = new Dictionary<string, string?>
            {
                { "Jwt:Secret", "this-is-a-long-enough-secret-key-for-HS256" },
                { "Jwt:Issuer", "fake-issuer" },
                { "Jwt:Audience", "fake-audience" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private static ControllerContext MockControllerContext(int userId, string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ====== AuthController Tests ======
        [Test]
        public async Task AuthController_Register_ShouldRegisterUser()
        {
            var controller = new AuthController(_context, _configuration);
            var userRegisterDto = new UserRegisterDTO
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password",
                Role = "User"
            };

            var result = await controller.Register(userRegisterDto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AuthController_Register_ShouldReturnBadRequest_WhenEmailExists()
        {
            var controller = new AuthController(_context, _configuration);
            var existingUser = new User { Email = "test@example.com", Password = "password" };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var userRegisterDto = new UserRegisterDTO
            {
                Name = "New User",
                Email = "test@example.com",
                Password = "password",
                Role = "User"
            };

            var result = await controller.Register(userRegisterDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task AuthController_Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var controller = new AuthController(_context, _configuration);
            var user = new User { Email = "test@example.com", Password = "password" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new UserLoginDTO { Email = "test@example.com", Password = "password" };

            var result = await controller.Login(loginDto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AuthController_Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            var controller = new AuthController(_context, _configuration);
            var loginDto = new UserLoginDTO { Email = "invalid@example.com", Password = "wrongpassword" };

            var result = await controller.Login(loginDto);

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        // ====== CartController Tests ======
        [Test]
        public async Task CartController_AddToCart_ShouldAddItem()
        {
            var controller = new CartController(_context);
            controller.ControllerContext = MockControllerContext(1, "User");

            var cartItemDto = new CartItemDTO { MenuItemId = 1, Quantity = 2, Price = 100 };
            var menuItem = new MenuItem { MenuItemId = 1, Name = "Pizza", Price = 100 };
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var cart = new Cart { CartId = 1, UserId = 1, TotalPrice = 0 };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await controller.AddToCart(cartItemDto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CartController_RemoveFromCart_ShouldRemoveItem()
        {
            var controller = new CartController(_context);
            controller.ControllerContext = MockControllerContext(1, "User");

            var cartItem = new CartItem { CartItemId = 1, CartId = 1, MenuItemId = 1, Quantity = 2, Price = 100 };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await controller.RemoveCartItem(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CartController_ClearCart_ShouldClearAllItems()
        {
            var controller = new CartController(_context);
            controller.ControllerContext = MockControllerContext(1, "User");

            var cart = new Cart { CartId = 1, UserId = 1, TotalPrice = 200 };
            var cartItem = new CartItem { CartItemId = 1, CartId = 1, MenuItemId = 1, Quantity = 2, Price = 100 };
            _context.Carts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await controller.ClearCart();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CartController_GetCart_ShouldReturnCartDetails()
        {
            var controller = new CartController(_context);
            controller.ControllerContext = MockControllerContext(1, "User");

            var cart = new Cart
            {
                CartId = 1,
                UserId = 1,
                TotalPrice = 200,
                CartItems = new List<CartItem>
                {
                    new CartItem { CartItemId = 1, MenuItemId = 1, Quantity = 2, Price = 100 }
                }
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await controller.GetCart();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        // ====== MenuItemController Tests ======
        [Test]
        public async Task MenuItemController_AddMenuItem_ShouldAddItem()
        {
            var controller = new MenuItemController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var menuItemDto = new MenuItemDTO
            {
                Name = "Pizza",
                Description = "Delicious Pizza",
                Category = "Dinner",
                Price = 100,
                Availability = true,
                RestaurantId = 1
            };

            var restaurant = new Restaurant { RestaurantId = 1, Name = "Test Restaurant" };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var result = await controller.AddMenuItem(menuItemDto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
        }

        [Test]
        public async Task MenuItemController_GetMenuItemsByRestaurant_ShouldReturnMenuItems()
        {
            var controller = new MenuItemController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var restaurant = new Restaurant { RestaurantId = 1, Name = "Test Restaurant", UserId = 1 };
            var menuItems = new List<MenuItem>
            {
                new MenuItem { MenuItemId = 1, Name = "Pizza", RestaurantId = 1 },
                new MenuItem { MenuItemId = 2, Name = "Burger", RestaurantId = 1 }
            };

            _context.Restaurants.Add(restaurant);
            _context.MenuItems.AddRange(menuItems);
            await _context.SaveChangesAsync();

            var result = await controller.GetMenuItemsByRestaurant(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task MenuItemController_GetMenuItemById_ShouldReturnMenuItem()
        {
            var controller = new MenuItemController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var menuItem = new MenuItem { MenuItemId = 1, Name = "Pizza", RestaurantId = 1 };
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var result = await controller.GetMenuItemById(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task MenuItemController_GetMenuItemById_ShouldReturnNotFound()
        {
            var controller = new MenuItemController(_context);

            var result = await controller.GetMenuItemById(99);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
        // ====== OrderController Tests ======
        [Test]
        public async Task OrderController_PlaceOrder_ShouldCreateOrder()
        {
            var controller = new OrderController(_context);
            controller.ControllerContext = MockControllerContext(1, "Customer");

            var orderDto = new OrderDTO
            {
                RestaurantId = 1,
                OrderDate = System.DateTime.Now,
                TotalAmount = 500,
                OrderStatus = "Placed",
                OrderItems = new List<OrderItemDTO>
                {
                    new OrderItemDTO { MenuItemId = 1, Quantity = 2, Price = 250 }
                }
            };

            var restaurant = new Restaurant { RestaurantId = 1, Name = "Test Restaurant" };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var result = await controller.PlaceOrder(orderDto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
        }

        [Test]
        public async Task OrderController_GetOrderById_ShouldReturnOrder()
        {
            var controller = new OrderController(_context);
            controller.ControllerContext = MockControllerContext(1, "Customer");

            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                TotalAmount = 500,
                OrderDate = System.DateTime.Now
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var result = await controller.GetOrderById(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task OrderController_GetOrderHistory_ShouldReturnUserOrders()
        {
            var controller = new OrderController(_context);
            controller.ControllerContext = MockControllerContext(1, "Customer");

            var orders = new List<Order>
            {
                new Order { OrderId = 1, UserId = 1, TotalAmount = 500 }
            };
            _context.Orders.AddRange(orders);
            await _context.SaveChangesAsync();

            var result = await controller.GetOrderHistory();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task OrderController_DeleteOrder_ShouldDeleteOrder()
        {
            var controller = new OrderController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var order = new Order
            {
                OrderId = 1,
                UserId = 1,
                TotalAmount = 500,
                OrderDate = System.DateTime.Now
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var result = await controller.DeleteOrder(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        // ====== RestaurantController Tests ======
        [Test]
        public async Task RestaurantController_AddRestaurant_ShouldAddRestaurant()
        {
            var controller = new RestaurantController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var restaurantDto = new RestaurantDTO
            {
                Name = "New Restaurant",
                Location = "123 Street",
                ContactNumber = "1234567890"
            };

            var result = await controller.AddRestaurant(restaurantDto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);
        }

        [Test]
        public async Task RestaurantController_GetAllRestaurants_ShouldReturnAllRestaurants()
        {
            var controller = new RestaurantController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var restaurants = new List<Restaurant>
            {
                new Restaurant { RestaurantId = 1, Name = "Test Restaurant 1" },
                new Restaurant { RestaurantId = 2, Name = "Test Restaurant 2" }
            };

            _context.Restaurants.AddRange(restaurants);
            await _context.SaveChangesAsync();

            var result = await controller.GetAllRestaurants();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task RestaurantController_GetRestaurantById_ShouldReturnRestaurant()
        {
            var controller = new RestaurantController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var restaurant = new Restaurant { RestaurantId = 1, Name = "Test Restaurant", UserId = 1 };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var result = await controller.GetRestaurantById(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task RestaurantController_DeleteRestaurant_ShouldDeleteRestaurant()
        {
            var controller = new RestaurantController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var restaurant = new Restaurant { RestaurantId = 1, Name = "Test Restaurant" };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var result = await controller.DeleteRestaurant(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        // ====== UserController Tests ======
        [Test]
        public async Task UserController_GetUserProfile_ShouldReturnProfile()
        {
            var controller = new UserController(_context);
            controller.ControllerContext = MockControllerContext(1, "Customer");

            var user = new User { UserId = 1, Name = "John Doe", Email = "john@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await controller.GetUserProfile();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UserController_UpdateUserProfile_ShouldUpdateProfile()
        {
            var controller = new UserController(_context);
            controller.ControllerContext = MockControllerContext(1, "Customer");

            var user = new User { UserId = 1, Name = "John Doe", Email = "john@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updatedUserDto = new UpdateUserDTO { Name = "John Updated", Email = "updated@example.com" };

            var result = await controller.UpdateUserProfile(updatedUserDto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UserController_DeleteUser_ShouldDeleteUser()
        {
            var controller = new UserController(_context);
            controller.ControllerContext = MockControllerContext(1, "Admin");

            var user = new User { UserId = 1, Name = "John Doe", Email = "john@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await controller.DeleteUser(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}

