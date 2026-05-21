using ProductCatalog.Core.Models.Entities;
using ProductCatalog.Core.Repositories;

namespace ProductCatalog.Core.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext dbContext, CategoryRepository categoryRepository)
    {
        if (dbContext.Products.Any()) return; // Already seeded

        // Seed Categories (pure in-memory via CategoryRepository)
        var categories = new List<Category>
        {
            // Root categories
            new() { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
            new() { Id = 2, Name = "Home & Garden", Description = "Home improvement and gardening supplies" },
            new() { Id = 3, Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" },
            new() { Id = 4, Name = "Books", Description = "Physical and digital books" },
            new() { Id = 5, Name = "Clothing", Description = "Apparel and fashion accessories" },

            // Sub-categories under Electronics
            new() { Id = 6, Name = "Computers", Description = "Laptops, desktops, and accessories", ParentCategoryId = 1 },
            new() { Id = 7, Name = "Phones", Description = "Smartphones and mobile accessories", ParentCategoryId = 1 },
            new() { Id = 8, Name = "Audio", Description = "Headphones, speakers, and audio equipment", ParentCategoryId = 1 },

            // Sub-categories under Computers
            new() { Id = 9, Name = "Laptops", Description = "Notebook computers and ultrabooks", ParentCategoryId = 6 },
            new() { Id = 10, Name = "Desktops", Description = "Desktop PCs and workstations", ParentCategoryId = 6 },

            // Sub-categories under Home & Garden
            new() { Id = 11, Name = "Furniture", Description = "Indoor and outdoor furniture", ParentCategoryId = 2 },
            new() { Id = 12, Name = "Garden Tools", Description = "Tools for gardening and landscaping", ParentCategoryId = 2 },
        };

        categoryRepository.SeedItems(categories);

        // Seed Products (EF Core In-Memory via AppDbContext)
        var products = new List<Product>
        {
            // Laptops (Category 9)
            new() { Id = 1, Name = "ProBook Ultra", Description = "14-inch business ultrabook with Intel Core i7", Sku = "LAPTOP-001", Price = 1299.99m, Quantity = 15, CategoryId = 9 },
            new() { Id = 2, Name = "GamerStorm X1", Description = "15.6-inch gaming laptop with RTX 4070", Sku = "LAPTOP-002", Price = 1899.99m, Quantity = 8, CategoryId = 9 },
            new() { Id = 3, Name = "ChromeBook Air", Description = "Lightweight Chromebook for students", Sku = "LAPTOP-003", Price = 349.99m, Quantity = 42, CategoryId = 9 },

            // Desktops (Category 10)
            new() { Id = 4, Name = "WorkStation Pro", Description = "High-performance desktop for creative professionals", Sku = "DESK-001", Price = 2499.99m, Quantity = 5, CategoryId = 10 },
            new() { Id = 5, Name = "HomeOffice Mini", Description = "Compact desktop for home office use", Sku = "DESK-002", Price = 599.99m, Quantity = 20, CategoryId = 10 },

            // Phones (Category 7)
            new() { Id = 6, Name = "PixelPhone 15", Description = "Flagship smartphone with AI camera", Sku = "PHONE-001", Price = 999.99m, Quantity = 30, CategoryId = 7 },
            new() { Id = 7, Name = "BudgetPhone 5G", Description = "Affordable 5G smartphone", Sku = "PHONE-002", Price = 299.99m, Quantity = 50, CategoryId = 7 },
            new() { Id = 8, Name = "RuggedPhone X", Description = "Military-grade rugged smartphone", Sku = "PHONE-003", Price = 449.99m, Quantity = 12, CategoryId = 7 },

            // Audio (Category 8)
            new() { Id = 9, Name = "NoiseCancel Pro", Description = "Premium noise-canceling headphones", Sku = "AUDIO-001", Price = 349.99m, Quantity = 25, CategoryId = 8 },
            new() { Id = 10, Name = "SportBuds Wireless", Description = "True wireless earbuds for sports", Sku = "AUDIO-002", Price = 129.99m, Quantity = 60, CategoryId = 8 },
            new() { Id = 11, Name = "BoomBox 360", Description = "Portable Bluetooth speaker", Sku = "AUDIO-003", Price = 79.99m, Quantity = 35, CategoryId = 8 },

            // Furniture (Category 11)
            new() { Id = 12, Name = "ErgoChair Plus", Description = "Ergonomic office chair with lumbar support", Sku = "FURN-001", Price = 449.99m, Quantity = 18, CategoryId = 11 },
            new() { Id = 13, Name = "Standing Desk", Description = "Electric height-adjustable standing desk", Sku = "FURN-002", Price = 699.99m, Quantity = 10, CategoryId = 11 },
            new() { Id = 14, Name = "Bookshelf Oak", Description = "Solid oak bookshelf with 5 shelves", Sku = "FURN-003", Price = 189.99m, Quantity = 22, CategoryId = 11 },

            // Garden Tools (Category 12)
            new() { Id = 15, Name = "PowerMower 3000", Description = "Electric lawn mower with 40cm cutting width", Sku = "GARD-001", Price = 329.99m, Quantity = 7, CategoryId = 12 },
            new() { Id = 16, Name = "HedgeTrimmer Pro", Description = "Cordless hedge trimmer with 55cm blade", Sku = "GARD-002", Price = 159.99m, Quantity = 14, CategoryId = 12 },

            // Sports (Category 3)
            new() { Id = 17, Name = "TrailRunner Shoes", Description = "Trail running shoes with waterproof membrane", Sku = "SPORT-001", Price = 139.99m, Quantity = 28, CategoryId = 3 },
            new() { Id = 18, Name = "YogaMat Premium", Description = "Extra-thick non-slip yoga mat", Sku = "SPORT-002", Price = 49.99m, Quantity = 45, CategoryId = 3 },
            new() { Id = 19, Name = "Camping Tent 4P", Description = "4-person dome tent with rainfly", Sku = "SPORT-003", Price = 199.99m, Quantity = 9, CategoryId = 3 },

            // Books (Category 4)
            new() { Id = 20, Name = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship", Sku = "BOOK-001", Price = 42.99m, Quantity = 100, CategoryId = 4 },
            new() { Id = 21, Name = "Design Patterns", Description = "Elements of Reusable Object-Oriented Software", Sku = "BOOK-002", Price = 54.99m, Quantity = 75, CategoryId = 4 },

            // Clothing (Category 5)
            new() { Id = 22, Name = "TechFleece Jacket", Description = "Lightweight breathable running jacket", Sku = "CLOTH-001", Price = 89.99m, Quantity = 33, CategoryId = 5 },
            new() { Id = 23, Name = "Cargo Pants", Description = "Durable cargo pants with multiple pockets", Sku = "CLOTH-002", Price = 59.99m, Quantity = 40, CategoryId = 5 },
        };

        foreach (var product in products)
        {
            product.CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365));
            product.UpdatedAt = product.CreatedAt.AddDays(Random.Shared.Next(1, 30));
        }

        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();
    }
}
