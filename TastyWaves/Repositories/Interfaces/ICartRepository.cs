using TastyWaves.Models;

namespace TastyWaves.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task AddCartAsync(Cart cart);
        Task UpdateCartAsync(Cart cart);
        Task ClearCartAsync(int cartId);
    }
}
