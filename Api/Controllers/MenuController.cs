using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Menu;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        #region Categories

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _menuService.GetAllCategoriesAsync();
            return Ok(ApiResponse<List<MenuCategoryDto>>.SuccessResponse(categories));
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            var category = await _menuService.GetCategoryByIdAsync(id);
            
            if (category == null)
                return NotFound(new ErrorResponse("Category not found"));
                
            return Ok(ApiResponse<MenuCategoryDto>.SuccessResponse(category));
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateMenuCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var category = await _menuService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, ApiResponse<MenuCategoryDto>.SuccessResponse(category, "Category created successfully"));
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateMenuCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var category = await _menuService.UpdateCategoryAsync(id, categoryDto);
            
            if (category == null)
                return NotFound(new ErrorResponse("Category not found"));
                
            return Ok(ApiResponse<MenuCategoryDto>.SuccessResponse(category, "Category updated successfully"));
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var result = await _menuService.DeleteCategoryAsync(id);
            
            if (!result)
                return NotFound(new ErrorResponse("Category not found or cannot be deleted"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully"));
        }

        #endregion

        #region MenuItems

        [HttpGet("items")]
        public async Task<IActionResult> GetAllMenuItems()
        {
            var menuItems = await _menuService.GetAllMenuItemsAsync();
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menuItems));
        }

        [HttpGet("items/category/{categoryId}")]
        public async Task<IActionResult> GetMenuItemsByCategory(string categoryId)
        {
            var menuItems = await _menuService.GetMenuItemsByCategoryAsync(categoryId);
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menuItems));
        }

        [HttpGet("items/featured")]
        public async Task<IActionResult> GetFeaturedMenuItems()
        {
            var menuItems = await _menuService.GetFeaturedMenuItemsAsync();
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menuItems));
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetMenuItemById(string id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            
            if (menuItem == null)
                return NotFound(new ErrorResponse("Menu item not found"));
                
            return Ok(ApiResponse<MenuItemDto>.SuccessResponse(menuItem));
        }

        [HttpGet("items/search")]
        public async Task<IActionResult> SearchMenuItems([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new ErrorResponse("Search query is required"));

            var menuItems = await _menuService.SearchMenuItemsAsync(query);
            return Ok(ApiResponse<List<MenuItemDto>>.SuccessResponse(menuItems));
        }

        [HttpPost("items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemDto menuItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var menuItem = await _menuService.CreateMenuItemAsync(menuItemDto);
            return CreatedAtAction(nameof(GetMenuItemById), new { id = menuItem.Id }, ApiResponse<MenuItemDto>.SuccessResponse(menuItem, "Menu item created successfully"));
        }

        [HttpPut("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMenuItem(string id, [FromBody] UpdateMenuItemDto menuItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var menuItem = await _menuService.UpdateMenuItemAsync(id, menuItemDto);
            
            if (menuItem == null)
                return NotFound(new ErrorResponse("Menu item not found"));
                
            return Ok(ApiResponse<MenuItemDto>.SuccessResponse(menuItem, "Menu item updated successfully"));
        }

        [HttpPatch("items/{id}/availability")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateMenuItemAvailability(string id, [FromBody] MenuItemAvailabilityDto availabilityDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _menuService.UpdateMenuItemAvailabilityAsync(id, availabilityDto);
            
            if (!result)
                return NotFound(new ErrorResponse("Menu item not found"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Menu item availability updated successfully"));
        }

        [HttpDelete("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItem(string id)
        {
            var result = await _menuService.DeleteMenuItemAsync(id);
            
            if (!result)
                return NotFound(new ErrorResponse("Menu item not found or cannot be deleted"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Menu item deleted successfully"));
        }

        #endregion
    }
}