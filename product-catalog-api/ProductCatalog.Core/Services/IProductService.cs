using ProductCatalog.Core.Models.Dtos;

namespace ProductCatalog.Core.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetAllAsync(ProductSearchFilters? filters);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);
}
