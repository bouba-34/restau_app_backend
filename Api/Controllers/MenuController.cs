using backend.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Menu;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly ImageHelper _imageHelper;


        public MenuController(IMenuService menuService, ImageHelper imageHelper)
        {
            _menuService = menuService;
            _imageHelper = imageHelper;
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
        public async Task<IActionResult> CreateCategoryWithImage([FromForm] CreateMenuCategoryDto categoryDto, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            if (image != null)
            {
                var imageUrl = await _imageHelper.UploadImageAsync(image, "categories");
                categoryDto.ImageUrl = imageUrl;
            }

            var category = await _menuService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, ApiResponse<MenuCategoryDto>.SuccessResponse(category, "Category created successfully"));
        }

        
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategoryWithImage(string id, [FromForm] UpdateMenuCategoryDto categoryDto, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            // Get existing category to check if we need to delete old image
            var existingCategory = await _menuService.GetCategoryByIdAsync(id);
            if (existingCategory == null)
                return NotFound(new ErrorResponse("Category not found"));

            if (image != null)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                {
                    await _imageHelper.DeleteImageAsync(existingCategory.ImageUrl);
                }

                var imageUrl = await _imageHelper.UploadImageAsync(image, "categories");
                categoryDto.ImageUrl = imageUrl;
            }

            var category = await _menuService.UpdateCategoryAsync(id, categoryDto);
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
        public async Task<IActionResult> CreateMenuItemWithImage([FromForm] CreateMenuItemDto menuItemDto, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            if (image != null)
            {
                var imageUrl = await _imageHelper.UploadImageAsync(image, "menu-items");
                menuItemDto.ImageUrl = imageUrl;
            }

            var menuItem = await _menuService.CreateMenuItemAsync(menuItemDto);
            return CreatedAtAction(nameof(GetMenuItemById), new { id = menuItem.Id }, ApiResponse<MenuItemDto>.SuccessResponse(menuItem, "Menu item created successfully"));
        }

        
        [HttpPut("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMenuItemWithImage(string id, [FromForm] UpdateMenuItemDto menuItemDto, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            // Get existing menu item to check if we need to delete old image
            var existingMenuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (existingMenuItem == null)
                return NotFound(new ErrorResponse("Menu item not found"));

            if (image != null)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingMenuItem.ImageUrl))
                {
                    await _imageHelper.DeleteImageAsync(existingMenuItem.ImageUrl);
                }

                var imageUrl = await _imageHelper.UploadImageAsync(image, "menu-items");
                menuItemDto.ImageUrl = imageUrl;
            }

            var menuItem = await _menuService.UpdateMenuItemAsync(id, menuItemDto);
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
        
        // Add a dedicated endpoint for deleting images if needed
        [HttpDelete("categories/{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoryImage(string id)
        {
            var category = await _menuService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new ErrorResponse("Category not found"));

            if (string.IsNullOrEmpty(category.ImageUrl))
                return BadRequest(new ErrorResponse("No image to delete"));

            var success = await _imageHelper.DeleteImageAsync(category.ImageUrl);
            if (!success)
                return BadRequest(new ErrorResponse("Failed to delete image"));

            // Update category to remove image URL
            var updateDto = new UpdateMenuCategoryDto
            {
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                ImageUrl = null,
                IsActive = category.IsActive
            };

            var updatedCategory = await _menuService.UpdateCategoryAsync(id, updateDto);
            return Ok(ApiResponse<MenuCategoryDto>.SuccessResponse(updatedCategory, "Category image deleted successfully"));
        }

        [HttpDelete("items/{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItemImage(string id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null)
                return NotFound(new ErrorResponse("Menu item not found"));

            if (string.IsNullOrEmpty(menuItem.ImageUrl))
                return BadRequest(new ErrorResponse("No image to delete"));

            var success = await _imageHelper.DeleteImageAsync(menuItem.ImageUrl);
            if (!success)
                return BadRequest(new ErrorResponse("Failed to delete image"));

            // Update menu item to remove image URL
            var updateDto = new UpdateMenuItemDto
            {
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                CategoryId = menuItem.CategoryId,
                ImageUrl = null,
                IsAvailable = menuItem.IsAvailable,
                IsVegetarian = menuItem.IsVegetarian,
                IsVegan = menuItem.IsVegan,
                IsGlutenFree = menuItem.IsGlutenFree,
                PreparationTimeMinutes = menuItem.PreparationTimeMinutes,
                Ingredients = menuItem.Ingredients,
                Allergens = menuItem.Allergens,
                Calories = menuItem.Calories,
                DiscountPercentage = menuItem.DiscountPercentage,
                IsFeatured = menuItem.IsFeatured,
                DisplayOrder = menuItem.DisplayOrder
            };

            var updatedMenuItem = await _menuService.UpdateMenuItemAsync(id, updateDto);
            return Ok(ApiResponse<MenuItemDto>.SuccessResponse(updatedMenuItem, "Menu item image deleted successfully"));
        }

        #endregion
    }
}