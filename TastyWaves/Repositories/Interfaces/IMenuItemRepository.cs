using TastyWaves.Models;

namespace TastyWaves.Repositories.Interfaces
{
    public interface IMenuItemRepository
    {
        Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId);
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<IEnumerable<MenuItem>> GetMenuItemsByCategoryAsync(string category);
        Task AddMenuItemAsync(MenuItem menuItem);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(int menuItemId);
    }
}
