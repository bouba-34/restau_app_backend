using Microsoft.EntityFrameworkCore;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;
using System.Linq.Expressions;

namespace backend.Api.Data.Repositories.Implementations
{
    public class MenuRepository : Repository<MenuItem>, IMenuRepository
    {
        public MenuRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MenuCategory>> GetAllCategoriesAsync()
        {
            return await _context.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MenuCategory> GetCategoryByIdAsync(string categoryId)
        {
            return await _context.MenuCategories.FindAsync(categoryId);
        }

        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string categoryId)
        {
            return await _context.MenuItems
                .Where(m => m.CategoryId == categoryId)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetFeaturedMenuItemsAsync()
        {
            return await _context.MenuItems
                .Where(m => m.IsFeatured && m.IsAvailable)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> SearchMenuItemsAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
                return new List<MenuItem>();

            query = query.ToLower();

            return await _context.MenuItems
                .Where(m => m.IsAvailable &&
                      (m.Name.ToLower().Contains(query) ||
                       m.Description.ToLower().Contains(query) ||
                       m.Ingredients.Any(i => i.ToLower().Contains(query))))
                .ToListAsync();
        }

        public async Task<string> AddCategoryAsync(MenuCategory category)
        {
            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();
            return category.Id;
        }

        public async Task<bool> UpdateCategoryAsync(MenuCategory category)
        {
            _context.MenuCategories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            // Check if any menu items use this category
            bool hasMenuItems = await _context.MenuItems.AnyAsync(m => m.CategoryId == categoryId);
            if (hasMenuItems)
                return false;

            var category = await _context.MenuCategories.FindAsync(categoryId);
            if (category == null)
                return false;

            _context.MenuCategories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMenuItemAvailabilityAsync(string menuItemId, bool isAvailable)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
                return false;

            menuItem.IsAvailable = isAvailable;
            menuItem.UpdatedAt = DateTime.UtcNow;

            _context.MenuItems.Update(menuItem);
            return await _context.SaveChangesAsync() > 0;
        }

        // Override the base class method to include category
        public override async Task<MenuItem> GetByIdAsync(string id)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Override to add additional filters or includes as needed
        public override async Task<IEnumerable<MenuItem>> GetAsync(
            Expression<Func<MenuItem, bool>> filter = null,
            Func<IQueryable<MenuItem>, IOrderedQueryable<MenuItem>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<MenuItem> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Always include Category
            query = query.Include(m => m.Category);

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (includeProperty != "Category") // Skip if already included
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                query = query.OrderBy(m => m.DisplayOrder);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
}