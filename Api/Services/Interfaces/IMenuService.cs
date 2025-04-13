using backend.Api.Models.DTOs.Menu;

namespace backend.Api.Services.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuCategoryDto>> GetAllCategoriesAsync();
        Task<MenuCategoryDto> GetCategoryByIdAsync(string categoryId);
        Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto categoryDto);
        Task<MenuCategoryDto> UpdateCategoryAsync(string categoryId, UpdateMenuCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(string categoryId);
        
        Task<List<MenuItemDto>> GetAllMenuItemsAsync();
        Task<List<MenuItemDto>> GetMenuItemsByCategoryAsync(string categoryId);
        Task<List<MenuItemDto>> GetFeaturedMenuItemsAsync();
        Task<MenuItemDto> GetMenuItemByIdAsync(string menuItemId);
        Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto menuItemDto);
        Task<MenuItemDto> UpdateMenuItemAsync(string menuItemId, UpdateMenuItemDto menuItemDto);
        Task<bool> DeleteMenuItemAsync(string menuItemId);
        Task<bool> UpdateMenuItemAvailabilityAsync(string menuItemId, MenuItemAvailabilityDto availabilityDto);
        Task<List<MenuItemDto>> SearchMenuItemsAsync(string query);
    }
}