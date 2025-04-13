using Microsoft.EntityFrameworkCore;
using backend.Api.Models.Entities;

namespace backend.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // MenuCategory configuration
            modelBuilder.Entity<MenuCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // MenuItem configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                
                // Category relationship
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.MenuItems)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Convert string arrays to JSON
                entity.Property(e => e.Ingredients).HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                
                entity.Property(e => e.Allergens).HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TipAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                // Customer relationship
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Staff relationship
                entity.HasOne(e => e.PreparedBy)
                      .WithMany()
                      .HasForeignKey(e => e.PreparedById)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                
                // Order relationship
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // MenuItem relationship
                entity.HasOne(e => e.MenuItem)
                      .WithMany()
                      .HasForeignKey(e => e.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Convert string arrays to JSON
                entity.Property(e => e.Customizations).HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            });

            // Reservation configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Customer relationship
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Table configuration
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.Number).IsUnique();
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                
                // User relationship
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Add admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = "admin-user-id",
                    Username = "admin",
                    Email = "admin@restaurant.com",
                    PhoneNumber = "1234567890",
                    // This is a hashed version of "Admin@123" - in reality, you would hash this properly
                    PasswordHash = "AQAAAAEAACcQAAAAEGa5MFgm5Rj0YJQ7zt7yYrlvY0rJi9YHdI5FhXVjuq5mPx3XXVnFEcUUJ3K0JQXmwQ==",
                    UserType = UserType.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
            
            // Add menu categories
            var appetizersCategoryId = "category-appetizers";
            var mainCoursesCategoryId = "category-main-courses";
            var dessertsCategoryId = "category-desserts";
            var beveragesCategoryId = "category-beverages";
            
            modelBuilder.Entity<MenuCategory>().HasData(
                new MenuCategory
                {
                    Id = appetizersCategoryId,
                    Name = "Appetizers",
                    Description = "Start your meal with something special",
                    DisplayOrder = 1,
                    ImageUrl = "/images/categories/appetizers.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuCategory
                {
                    Id = mainCoursesCategoryId,
                    Name = "Main Courses",
                    Description = "Delicious entrées for every taste",
                    DisplayOrder = 2,
                    ImageUrl = "/images/categories/main-courses.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuCategory
                {
                    Id = dessertsCategoryId,
                    Name = "Desserts",
                    Description = "Sweet treats to complete your meal",
                    DisplayOrder = 3,
                    ImageUrl = "/images/categories/desserts.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuCategory
                {
                    Id = beveragesCategoryId,
                    Name = "Beverages",
                    Description = "Refreshing drinks for any occasion",
                    DisplayOrder = 4,
                    ImageUrl = "/images/categories/beverages.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            
            // Add some menu items
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem
                {
                    Id = "item-caesar-salad",
                    Name = "Caesar Salad",
                    Description = "Crisp romaine lettuce, parmesan cheese, croutons, and our special dressing",
                    Price = 8.99m,
                    ImageUrl = "/images/menu/caesar-salad.jpg",
                    IsAvailable = true,
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    PreparationTimeMinutes = 10,
                    CategoryId = appetizersCategoryId,
                    Calories = 350,
                    DiscountPercentage = 0,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = "item-margherita-pizza",
                    Name = "Margherita Pizza",
                    Description = "Classic pizza with tomato sauce, fresh mozzarella, and basil",
                    Price = 12.99m,
                    ImageUrl = "/images/menu/margherita-pizza.jpg",
                    IsAvailable = true,
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    PreparationTimeMinutes = 15,
                    CategoryId = mainCoursesCategoryId,
                    Calories = 800,
                    DiscountPercentage = 0,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = "item-chocolate-cake",
                    Name = "Chocolate Cake",
                    Description = "Rich, moist chocolate cake with a smooth frosting",
                    Price = 6.99m,
                    ImageUrl = "/images/menu/chocolate-cake.jpg",
                    IsAvailable = true,
                    IsVegetarian = true,
                    IsVegan = false,
                    IsGlutenFree = false,
                    PreparationTimeMinutes = 5,
                    CategoryId = dessertsCategoryId,
                    Calories = 450,
                    DiscountPercentage = 0,
                    IsFeatured = false,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = "item-lemonade",
                    Name = "Fresh Lemonade",
                    Description = "Freshly squeezed lemons with the perfect balance of sweet and tart",
                    Price = 3.50m,
                    ImageUrl = "/images/menu/lemonade.jpg",
                    IsAvailable = true,
                    IsVegetarian = true,
                    IsVegan = true,
                    IsGlutenFree = true,
                    PreparationTimeMinutes = 3,
                    CategoryId = beveragesCategoryId,
                    Calories = 120,
                    DiscountPercentage = 0,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            
            // Seed tables
            for (int i = 1; i <= 10; i++)
            {
                modelBuilder.Entity<Table>().HasData(
                    new Table
                    {
                        Id = $"table-{i}",
                        Number = i.ToString(),
                        Capacity = i <= 4 ? 4 : (i <= 8 ? 6 : 8), // Tables 1-4: 4 people, 5-8: 6 people, 9-10: 8 people
                        Status = TableStatus.Available,
                        Location = i <= 5 ? "Main Area" : (i <= 8 ? "Window Side" : "Private Area"),
                        IsActive = true
                    }
                );
            }
        }
    }
}