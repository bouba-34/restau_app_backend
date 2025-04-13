using AutoMapper;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Helpers;
using backend.Api.Hubs;
using backend.Api.Models.DTOs.Menu;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace backend.Api.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<RestaurantHub> _hubContext;
        private readonly ImageHelper _imageHelper;


        public MenuService(
            IMenuRepository menuRepository,
            IMapper mapper,
            IHubContext<RestaurantHub> hubContext,
            ImageHelper imageHelper)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
            _hubContext = hubContext;
            _imageHelper = imageHelper;
        }


        public async Task<List<MenuCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _menuRepository.GetAllCategoriesAsync();
            return _mapper.Map<List<MenuCategoryDto>>(categories);
        }

        public async Task<MenuCategoryDto> GetCategoryByIdAsync(string categoryId)
        {
            var category = await _menuRepository.GetCategoryByIdAsync(categoryId);
            return _mapper.Map<MenuCategoryDto>(category);
        }

        public async Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto categoryDto)
        {
            var category = _mapper.Map<MenuCategory>(categoryDto);
            category.Id = Guid.NewGuid().ToString();
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.AddCategoryAsync(category);
            return _mapper.Map<MenuCategoryDto>(category);
        }

        public async Task<MenuCategoryDto> UpdateCategoryAsync(string categoryId, UpdateMenuCategoryDto categoryDto)
        {
            var existingCategory = await _menuRepository.GetCategoryByIdAsync(categoryId);
            if (existingCategory == null)
                return null;

            _mapper.Map(categoryDto, existingCategory);
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.UpdateCategoryAsync(existingCategory);
            return _mapper.Map<MenuCategoryDto>(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            // Get category to find image URL
            var category = await _menuRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                return false;

            // Delete image if exists
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                await _imageHelper.DeleteImageAsync(category.ImageUrl);
            }

            return await _menuRepository.DeleteCategoryAsync(categoryId);
        }

        public async Task<List<MenuItemDto>> GetAllMenuItemsAsync()
        {
            var menuItems = await _menuRepository.GetAllAsync();
            return _mapper.Map<List<MenuItemDto>>(menuItems);
        }

        public async Task<List<MenuItemDto>> GetMenuItemsByCategoryAsync(string categoryId)
        {
            var menuItems = await _menuRepository.GetMenuItemsByCategoryAsync(categoryId);
            return _mapper.Map<List<MenuItemDto>>(menuItems);
        }

        public async Task<List<MenuItemDto>> GetFeaturedMenuItemsAsync()
        {
            var menuItems = await _menuRepository.GetFeaturedMenuItemsAsync();
            return _mapper.Map<List<MenuItemDto>>(menuItems);
        }

        public async Task<MenuItemDto> GetMenuItemByIdAsync(string menuItemId)
        {
            var menuItem = await _menuRepository.GetByIdAsync(menuItemId);
            return _mapper.Map<MenuItemDto>(menuItem);
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto menuItemDto)
        {
            var menuItem = _mapper.Map<MenuItem>(menuItemDto);
            menuItem.Id = Guid.NewGuid().ToString();
            menuItem.CreatedAt = DateTime.UtcNow;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.AddAsync(menuItem);
            return _mapper.Map<MenuItemDto>(menuItem);
        }

        public async Task<MenuItemDto> UpdateMenuItemAsync(string menuItemId, UpdateMenuItemDto menuItemDto)
        {
            var existingMenuItem = await _menuRepository.GetByIdAsync(menuItemId);
            if (existingMenuItem == null)
                return null;

            _mapper.Map(menuItemDto, existingMenuItem);
            existingMenuItem.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.UpdateAsync(existingMenuItem);
            return _mapper.Map<MenuItemDto>(existingMenuItem);
        }

        public async Task<bool> DeleteMenuItemAsync(string menuItemId)
        {
            // Get menu item to find image URL
            var menuItem = await _menuRepository.GetByIdAsync(menuItemId);
            if (menuItem == null)
                return false;

            // Delete image if exists
            if (!string.IsNullOrEmpty(menuItem.ImageUrl))
            {
                await _imageHelper.DeleteImageAsync(menuItem.ImageUrl);
            }

            return await _menuRepository.RemoveAsync(menuItemId);
        }

        public async Task<bool> UpdateMenuItemAvailabilityAsync(string menuItemId, MenuItemAvailabilityDto availabilityDto)
        {
            var success = await _menuRepository.UpdateMenuItemAvailabilityAsync(menuItemId, availabilityDto.IsAvailable);
            
            if (success)
            {
                // Notify clients through SignalR
                await _hubContext.Clients.All.SendAsync("MenuItemAvailabilityChanged", menuItemId, availabilityDto.IsAvailable);
            }
            
            return success;
        }

        public async Task<List<MenuItemDto>> SearchMenuItemsAsync(string query)
        {
            var menuItems = await _menuRepository.SearchMenuItemsAsync(query);
            return _mapper.Map<List<MenuItemDto>>(menuItems);
        }
    }
}