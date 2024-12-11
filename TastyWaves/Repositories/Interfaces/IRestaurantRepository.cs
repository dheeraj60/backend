using TastyWaves.Models;

namespace TastyWaves.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<Restaurant?> GetRestaurantByIdAsync(int restaurantId);
        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
        Task AddRestaurantAsync(Restaurant restaurant);
        Task UpdateRestaurantAsync(Restaurant restaurant);
        Task DeleteRestaurantAsync(int restaurantId);
    }
}
