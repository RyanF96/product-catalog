using Microsoft.EntityFrameworkCore;
using ProductCatalog.Core.Data;
using ProductCatalog.Core.Models.Entities;

namespace ProductCatalog.Core.Repositories;

/// <summary>
/// Product repository backed by EF Core In-Memory database.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
        => await _context.Products
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids)
        => await _context.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product> AddAsync(Product entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity is not null)
        {
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Products.AnyAsync(p => p.Id == id);

    public async Task<int> CountAsync()
        => await _context.Products.CountAsync();
}
