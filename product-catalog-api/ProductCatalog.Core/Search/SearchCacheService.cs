using ProductCatalog.Core.Models.Dtos;

namespace ProductCatalog.Core.Search;

/// <summary>
/// Simple in-memory caching layer for search results.
/// Uses Dictionary&lt;TKey, TValue&gt; with TTL-based expiration.
/// </summary>
public class SearchCacheService
{
    private readonly Dictionary<string, CacheEntry<IReadOnlyList<ProductDto>>> _cache = new();
    private readonly object _lock = new();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(60);

    public IReadOnlyList<ProductDto>? Get(string key)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (DateTime.UtcNow < entry.ExpiresAt)
                {
                    return entry.Value;
                }

                // Expired — remove
                _cache.Remove(key);
            }

            return null;
        }
    }

    public void Set(string key, IReadOnlyList<ProductDto> value)
    {
        lock (_lock)
        {
            _cache[key] = new CacheEntry<IReadOnlyList<ProductDto>>(
                value,
                DateTime.UtcNow.Add(_ttl)
            );
        }
    }

    public void Invalidate(string pattern)
    {
        lock (_lock)
        {
            var keysToRemove = _cache.Keys
                .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    public static string BuildKey(ProductSearchFilters filters)
    {
        return $"search:{filters.SearchTerm ?? "*"}:cat:{filters.CategoryId ?? 0}:price:{filters.MinPrice}-{filters.MaxPrice}:stock:{filters.InStockOnly}";
    }
}
