using ProductCatalog.Core.Models.Entities;
using ProductCatalog.Core.Search;

namespace ProductCatalog.Api.Tests;

public class ProductSearchEngineTests
{
    private readonly ProductSearchEngine _engine;

    public ProductSearchEngineTests()
    {
        _engine = new ProductSearchEngine();
    }

    private static Product CreateProduct(int id, string name, string sku, string description)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Sku = sku,
            Description = description,
            Price = 99.99m,
            Quantity = 10,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Search_ExactMatch_ReturnsResults()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "ProBook Ultra", "LAPTOP-001", "Business laptop"),
            CreateProduct(2, "GamerStorm X1", "DESK-001", "Gaming desktop"),
            CreateProduct(3, "BudgetPhone 5G", "PHONE-001", "Affordable smartphone")
        };
        _engine.BuildIndex(products);

        // Act
        var results = _engine.Search("ProBook").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("ProBook Ultra", results[0].Name);
    }

    [Theory]
    [InlineData("lptop", "ProBook Ultra")]           // typo: lptop -> laptop
    [InlineData("laptop", "ProBook Ultra")]          // exact partial
    [InlineData("phon", "Budget Phone 5G")]          // prefix match
    [InlineData("bidget", "Budget Phone 5G")]        // typo: bidget -> budget
    [InlineData("gamerstrom", "Gamer Storm X1")]     // typo: gamerstrom -> gamerstorm
    public void Search_FuzzyMatching_FindsTypoTolerantResults(string query, string expectedName)
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "ProBook Ultra", "LAPTOP-001", "Business laptop with great performance"),
            CreateProduct(2, "Gamer Storm X1", "DESK-001", "Gaming desktop PC"),
            CreateProduct(3, "Budget Phone 5G", "PHONE-001", "Affordable 5G smartphone for everyone")
        };
        _engine.BuildIndex(products);

        // Act
        var results = _engine.Search(query).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(expectedName, results[0].Name);
    }

    [Fact]
    public void Search_MultiFieldWeightedScoring_NameHigherThanDescription()
    {
        // Arrange: Product A has match in name, Product B has match only in description
        var products = new List<Product>
        {
            CreateProduct(1, "NoiseCancel Headphones", "AUDIO-001", "Basic audio device"),
            CreateProduct(2, "Basic Speaker", "AUDIO-002", "NoiseCancel technology for pure sound")
        };
        _engine.BuildIndex(products);

        // Act
        var results = _engine.Search("NoiseCancel").ToList();

        // Assert: Name match should score higher than description match
        Assert.True(results.Count >= 2);
        Assert.Equal("NoiseCancel Headphones", results[0].Name);
    }

    [Fact]
    public void Search_NoResults_ReturnsEmpty()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "ProBook Ultra", "LAPTOP-001", "Business laptop")
        };
        _engine.BuildIndex(products);

        // Act
        var results = _engine.Search("xyznonexistent").ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsAll()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Product A", "SKU-001", "Description A"),
            CreateProduct(2, "Product B", "SKU-002", "Description B"),
            CreateProduct(3, "Product C", "SKU-003", "Description C")
        };
        _engine.BuildIndex(products);

        // Act
        var results = _engine.Search("").ToList();

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void AddProduct_AfterBuildIndex_IncludesNewProduct()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(1, "Alpha Widget", "SKU-001", "Description A")
        };
        _engine.BuildIndex(products);

        // Act
        _engine.AddProduct(CreateProduct(2, "Beta Gadget", "SKU-002", "Description B"));
        var results = _engine.Search("Beta").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Beta Gadget", results[0].Name);
    }
}
