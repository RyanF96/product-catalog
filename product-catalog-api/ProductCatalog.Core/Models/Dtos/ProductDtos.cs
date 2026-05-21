namespace ProductCatalog.Core.Models.Dtos;

// --- Read DTOs ---

public record ProductDto(
    int Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int Quantity,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

// --- Write DTOs ---

public record CreateProductRequest(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int Quantity,
    int CategoryId
);

public record UpdateProductRequest(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int Quantity,
    int CategoryId
);

// --- Search / Filter DTOs ---

public record ProductSearchFilters(
    string? SearchTerm,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? InStockOnly,
    int PageNumber = 1,
    int PageSize = 10
);

// --- Cache Entry Wrapper ---

public record CacheEntry<T>(T Value, DateTime ExpiresAt);
