using AdvancedEFCoreDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreDemo.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Makes sure the database exists and all migrations are applied.
        await context.Database.MigrateAsync();

        if (await context.Categories.AnyAsync())
        {
            return; // already seeded
        }

        var electronics = new Category { Name = "Electronics" };
        var accessories = new Category { Name = "Accessories" };
        var furniture = new Category { Name = "Furniture" };

        context.Categories.AddRange(electronics, accessories, furniture);

        context.Products.AddRange(
            new Product { Name = "Laptop", Price = 20000m, Category = electronics },
            new Product { Name = "Laptop Bag", Price = 600m, Category = accessories },
            new Product { Name = "Laptop Stand", Price = 450m, Category = accessories },
            new Product { Name = "Smartphone", Price = 12000m, Category = electronics },
            new Product { Name = "Tablet", Price = 8000m, Category = electronics },
            new Product { Name = "Monitor", Price = 5000m, Category = electronics },
            new Product { Name = "Keyboard", Price = 200m, Category = accessories },
            new Product { Name = "Mouse", Price = 50m, Category = accessories },
            new Product { Name = "Headphones", Price = 900m, Category = accessories },
            new Product { Name = "USB Cable", Price = 30m, Category = accessories },
            new Product { Name = "Office Chair", Price = 3500m, Category = furniture },
            new Product { Name = "Desk", Price = 4200m, Category = furniture },
            new Product { Name = "Bookshelf", Price = 2800m, Category = furniture },
            new Product { Name = "Desk Lamp", Price = 350m, Category = furniture },
            new Product { Name = "Webcam", Price = 700m, Category = electronics });

        await context.SaveChangesAsync();
    }
}
