using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Core.Search;

/// <summary>
/// DI-injected search engine for Product entities.
/// Uses the generic SearchEngine&lt;T&gt; with multi-field weighted scoring.
/// </summary>
public class ProductSearchEngine
{
    private readonly SearchEngine<Product> _engine;

    public ProductSearchEngine()
    {
        // Multi-field search with weighted scoring:
        // - Name: 50% weight (most important)
        // - SKU: 30% weight
        // - Description: 20% weight
        _engine = new SearchEngine<Product>(
            fieldSelectors: new Func<Product, string>[]
            {
                p => p.Name,
                p => p.Sku,
                p => p.Description
            },
            fieldWeights: new[] { 0.5, 0.3, 0.2 }
        );
    }

    public void BuildIndex(IEnumerable<Product> products)
    {
        _engine.BuildIndex(products);
    }

    public void AddProduct(Product product)
    {
        _engine.AddItem(product);
    }

    public void RemoveProduct(Product product)
    {
        _engine.RemoveItem(product);
    }

    public IEnumerable<Product> Search(string query)
    {
        return _engine.Search(query, fuzzyThreshold: 0.75);
    }

    /// <summary>
    /// Maps raw Product search results to DTOs with relevance scoring.
    /// </summary>

}
