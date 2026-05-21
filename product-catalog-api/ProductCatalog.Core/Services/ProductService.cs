using ProductCatalog.Core.Extensions;
using ProductCatalog.Core.Models.Dtos;
using ProductCatalog.Core.Models.Entities;
using ProductCatalog.Core.Repositories;
using ProductCatalog.Core.Search;

namespace ProductCatalog.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ProductSearchEngine _searchEngine;
    private readonly SearchCacheService _cache;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ProductSearchEngine searchEngine,
        SearchCacheService cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _searchEngine = searchEngine;
        _cache = cache;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(ProductSearchFilters? filters)
    {
        filters ??= new ProductSearchFilters(null, null, null, null, null);

        // Check cache first (key is pagination-agnostic)
        var cacheKey = SearchCacheService.BuildKey(filters);
        var cached = _cache.Get(cacheKey);
        if (cached is not null)
        {
            return new PagedResultDto<ProductDto>(
                cached.Paginate(filters.PageNumber, filters.PageSize).ToList(),
                cached.Count,
                filters.PageNumber,
                filters.PageSize
            );
        }

        // Choose data source: search engine when term is provided, otherwise repository
        IEnumerable<Product> query = !string.IsNullOrWhiteSpace(filters.SearchTerm)
            ? _searchEngine.Search(filters.SearchTerm)
            : await _productRepository.GetAllAsync();

        // Apply filters using custom LINQ extensions + pattern matching
        query = filters switch
        {
            { CategoryId: not null } => query.FilterByCategory(filters.CategoryId.Value),
            _ => query
        };

        query = filters switch
        {
            { MinPrice: not null, MaxPrice: not null }
                => query.FilterByPriceRange(filters.MinPrice.Value, filters.MaxPrice.Value),
            { MinPrice: not null }
                => query.FilterByPriceRange(filters.MinPrice.Value, decimal.MaxValue),
            { MaxPrice: not null }
                => query.FilterByPriceRange(decimal.MinValue, filters.MaxPrice.Value),
            _ => query
        };

        query = filters switch
        {
            { InStockOnly: true } => query.FilterInStock(),
            _ => query
        };

        var filteredProducts = query.ToList();
        var categoryIds = filteredProducts.Select(p => p.CategoryId).Distinct();
        var categoryNames = await GetCategoryNamesAsync(categoryIds);
        var allDtos = filteredProducts.Select(p => MapToDto(p, categoryNames)).ToList();

        // Cache the full filtered result (before pagination)
        _cache.Set(cacheKey, allDtos);

        var totalCount = allDtos.Count;
        var pagedItems = allDtos
            .Paginate(filters.PageNumber, filters.PageSize)
            .ToList();

        return new PagedResultDto<ProductDto>(pagedItems, totalCount, filters.PageNumber, filters.PageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null) return null;

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        return MapToDto(product, category?.Name ?? "Unknown");
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        // Pattern matching validation
        if (request is not { Name: not null and not "", Sku: not null and not "", Price: > 0, Quantity: >= 0 })
        {
            throw new ArgumentException("Invalid product data. Name and SKU are required, Price must be positive, Quantity must be non-negative.");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = request.Sku,
            Price = request.Price,
            Quantity = request.Quantity,
            CategoryId = request.CategoryId
        };

        var created = await _productRepository.AddAsync(product);
        _searchEngine.AddProduct(created);
        _cache.Invalidate("search");

        var category = await _categoryRepository.GetByIdAsync(created.CategoryId);
        return MapToDto(created, category?.Name ?? "Unknown");
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
            return null;

        // Pattern matching validation
        if (request is not { Name: not null and not "", Sku: not null and not "", Price: > 0, Quantity: >= 0 })
        {
            throw new ArgumentException("Invalid product data. Name and SKU are required, Price must be positive, Quantity must be non-negative.");
        }

        var oldProduct = new Product { Id = existing.Id }; // for search engine removal

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Sku = request.Sku;
        existing.Price = request.Price;
        existing.Quantity = request.Quantity;
        existing.CategoryId = request.CategoryId;

        await _productRepository.UpdateAsync(existing);
        _searchEngine.RemoveProduct(oldProduct);
        _searchEngine.AddProduct(existing);
        _cache.Invalidate("search");

        var category = await _categoryRepository.GetByIdAsync(existing.CategoryId);
        return MapToDto(existing, category?.Name ?? "Unknown");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
            return false;

        await _productRepository.DeleteAsync(id);
        _searchEngine.RemoveProduct(existing);
        _cache.Invalidate("search");

        return true;
    }

    private async Task<Dictionary<int, string>> GetCategoryNamesAsync(IEnumerable<int> categoryIds)
    {
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
        return categories.ToDictionary(c => c.Id, c => c.Name);
    }

    private static ProductDto MapToDto(Product p, string categoryName)
    {
        return new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Sku,
            p.Price,
            p.Quantity,
            p.CategoryId,
            categoryName,
            p.CreatedAt,
            p.UpdatedAt
        );
    }

    private static ProductDto MapToDto(Product p, Dictionary<int, string> categoryNames)
    {
        return new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Sku,
            p.Price,
            p.Quantity,
            p.CategoryId,
            categoryNames.GetValueOrDefault(p.CategoryId, "Unknown"),
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
