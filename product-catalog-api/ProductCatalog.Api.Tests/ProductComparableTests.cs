using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Api.Tests;

public class ProductComparableTests
{
    private static Product CreateProduct(int id, string name, decimal price)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = "Test",
            Sku = $"SKU-{id:D3}",
            Price = price,
            Quantity = 10,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void CompareTo_SamePrice_DifferentName_SortsAlphabetically()
    {
        // Arrange
        var productA = CreateProduct(1, "Alpha", 100m);
        var productB = CreateProduct(2, "Beta", 100m);

        // Act & Assert
        Assert.True(productA.CompareTo(productB) < 0);
        Assert.True(productB.CompareTo(productA) > 0);
    }

    [Fact]
    public void CompareTo_DifferentPrice_SortsByPriceAscending()
    {
        // Arrange
        var cheap = CreateProduct(1, "Zebra", 50m);
        var expensive = CreateProduct(2, "Apple", 200m);

        // Act & Assert
        Assert.True(cheap.CompareTo(expensive) < 0);
        Assert.True(expensive.CompareTo(cheap) > 0);
    }

    [Fact]
    public void CompareTo_SamePriceAndName_ReturnsZero()
    {
        // Arrange
        var a = CreateProduct(1, "Same", 100m);
        var b = CreateProduct(2, "Same", 100m);

        // Act & Assert
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        // Arrange
        var product = CreateProduct(1, "Test", 100m);

        // Act & Assert
        Assert.True(product.CompareTo(null) > 0);
    }

    [Fact]
    public void Sort_ByPriceThenName_ProducesCorrectOrder()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Gamma", 200m),
            CreateProduct(2, "Alpha", 100m),
            CreateProduct(3, "Beta", 100m),
            CreateProduct(4, "Delta", 50m)
        };

        // Act
        products.Sort();

        // Assert: Should be sorted by Price asc, then Name asc
        Assert.Equal("Delta", products[0].Name);   // 50m
        Assert.Equal("Alpha", products[1].Name);   // 100m
        Assert.Equal("Beta", products[2].Name);    // 100m
        Assert.Equal("Gamma", products[3].Name);   // 200m
    }

    [Fact]
    public void OrderBy_UsingIComparable_SortsCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Zebra", 999.99m),
            CreateProduct(2, "Apple", 9.99m),
            CreateProduct(3, "Mango", 99.99m)
        };

        // Act
        var sorted = products.OrderBy(p => p).ToList();

        // Assert
        Assert.Equal("Apple", sorted[0].Name);   // 9.99m
        Assert.Equal("Mango", sorted[1].Name);   // 99.99m
        Assert.Equal("Zebra", sorted[2].Name);   // 999.99m
    }
}
