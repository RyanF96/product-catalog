using ProductCatalog.Core.Extensions;
using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Api.Tests;

public class LinqExtensionTests
{
    private static Product CreateProduct(int id, string name, decimal price, int quantity, int categoryId)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = "Test description",
            Sku = $"SKU-{id:D3}",
            Price = price,
            Quantity = quantity,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void FilterByCategory_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Laptop", 999.99m, 10, 1),
            CreateProduct(2, "Phone", 599.99m, 5, 2),
            CreateProduct(3, "Tablet", 399.99m, 8, 1)
        };

        // Act
        var result = products.FilterByCategory(1).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(1, p.CategoryId));
    }

    [Theory]
    [InlineData(100, 500, 2)]   // Phone + Tablet
    [InlineData(0, 200, 0)]     // Nothing
    [InlineData(900, 2000, 1)]  // Just Laptop
    public void FilterByPriceRange_ReturnsCorrectProducts(decimal min, decimal max, int expectedCount)
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Laptop", 999.99m, 10, 1),
            CreateProduct(2, "Phone", 299.99m, 5, 2),
            CreateProduct(3, "Tablet", 399.99m, 8, 1)
        };

        // Act
        var result = products.FilterByPriceRange(min, max).ToList();

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void FilterInStock_ReturnsOnlyProductsWithPositiveQuantity()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "In Stock", 99.99m, 10, 1),
            CreateProduct(2, "Out of Stock", 49.99m, 0, 1),
            CreateProduct(3, "Also In Stock", 29.99m, 5, 2),
            CreateProduct(4, "Zero Stock", 19.99m, 0, 2)
        };

        // Act
        var result = products.FilterInStock().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.Quantity > 0));
    }

    [Theory]
    [InlineData("lap", 1)]       // partial match
    [InlineData("Laptop", 1)]    // exact case-insensitive
    [InlineData("LAPTOP", 1)]    // uppercase
    [InlineData("xyz", 0)]       // no match
    [InlineData("", 2)]          // empty returns all
    public void SearchByName_ReturnsCorrectResults(string searchTerm, int expectedCount)
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Laptop Pro", 999.99m, 10, 1),
            CreateProduct(2, "Desktop Elite", 1299.99m, 5, 2)
        };

        // Act
        var result = products.SearchByName(searchTerm).ToList();

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData(1, 5, 5)]   // page 1, size 5 -> 5 items
    [InlineData(2, 5, 3)]   // page 2, size 5 -> 3 items (of 8)
    [InlineData(1, 20, 8)]  // page 1, size 20 -> all 8 items
    public void Paginate_ReturnsCorrectPage(int page, int size, int expectedCount)
    {
        // Arrange
        var products = Enumerable.Range(1, 8)
            .Select(i => CreateProduct(i, $"Product {i}", i * 10m, i, 1))
            .ToList();

        // Act
        var result = products.Paginate(page, size).ToList();

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void Paginate_InvalidPage_ReturnsPage1()
    {
        // Arrange
        var products = new List<Product> { CreateProduct(1, "A", 10m, 1, 1) };

        // Act
        var result = products.Paginate(0, 10).ToList();

        // Assert
        Assert.Single(result);
    }
}
