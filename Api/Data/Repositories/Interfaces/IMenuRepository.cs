using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface IMenuRepository : IRepository<MenuItem>
    {
        Task<List<MenuCategory>> GetAllCategoriesAsync();
        Task<MenuCategory> GetCategoryByIdAsync(string categoryId);
        Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string categoryId);
        Task<List<MenuItem>> GetFeaturedMenuItemsAsync();
        Task<List<MenuItem>> SearchMenuItemsAsync(string query);
        Task<string> AddCategoryAsync(MenuCategory category);
        Task<bool> UpdateCategoryAsync(MenuCategory category);
        Task<bool> DeleteCategoryAsync(string categoryId);
        Task<bool> UpdateMenuItemAvailabilityAsync(string menuItemId, bool isAvailable);
    }
}