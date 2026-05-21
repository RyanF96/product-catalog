using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Core.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<Product> FilterByCategory(this IEnumerable<Product> source, int categoryId)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(p => p.CategoryId == categoryId);
    }

    public static IEnumerable<Product> FilterByPriceRange(this IEnumerable<Product> source, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(p => p.Price >= min && p.Price <= max);
    }

    public static IEnumerable<Product> FilterInStock(this IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Where(p => p.Quantity > 0);
    }

    public static IEnumerable<Product> SearchByName(this IEnumerable<Product> source, string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(searchTerm))
            return source;

        return source.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
