using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Core.Repositories;

/// <summary>
/// Category repository using pure in-memory collections (List&lt;T&gt;).
/// Satisfies the requirement: "At least one repository must use pure in-memory
/// collections (List, Dictionary) instead of Entity Framework."
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    protected readonly List<Category> _items = new();
    protected readonly object _lock = new();

    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Category>>(_items.ToList());
        }
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<IReadOnlyList<Category>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idSet = new HashSet<int>(ids);
        lock (_lock)
        {
            var items = _items.Where(c => idSet.Contains(c.Id)).ToList();
            return Task.FromResult<IReadOnlyList<Category>>(items);
        }
    }

    public Task<Category> AddAsync(Category entity)
    {
        lock (_lock)
        {
            _items.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task UpdateAsync(Category entity)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(c => c.Id == entity.Id);
            if (index >= 0)
            {
                _items[index] = entity;
            }
            return Task.CompletedTask;
        }
    }

    public Task DeleteAsync(int id)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(c => c.Id == id);
            if (index >= 0)
            {
                _items.RemoveAt(index);
            }
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(_items.Any(c => c.Id == id));
        }
    }

    public Task<int> CountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_items.Count);
        }
    }

    // Internal method for seeding data
    internal void SeedItems(IEnumerable<Category> categories)
    {
        lock (_lock)
        {
            _items.AddRange(categories);
        }
    }
}
