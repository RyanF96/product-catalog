using ProductCatalog.Core.Models.Dtos;
using ProductCatalog.Core.Models.Entities;
using ProductCatalog.Core.Repositories;

namespace ProductCatalog.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<CategoryTreeNode>> GetTreeAsync()
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var lookup = allCategories.ToLookup(c => c.ParentCategoryId);
        return BuildTreeNodes(lookup, null);
    }

    private static List<CategoryTreeNode> BuildTreeNodes(ILookup<int?, Category> lookup, int? parentId)
    {
        return lookup[parentId]
            .Select(c => new CategoryTreeNode(
                c.Id,
                c.Name,
                c.Description,
                BuildTreeNodes(lookup, c.Id)
            ))
            .ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        ValidateRequest(request);

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };

        var created = await _categoryRepository.AddAsync(category);
        return MapToDto(created);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var existing = await _categoryRepository.GetByIdAsync(id);
        if (existing is null)
            return null;

        ValidateRequest(request);

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.ParentCategoryId = request.ParentCategoryId;

        await _categoryRepository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _categoryRepository.ExistsAsync(id))
            return false;

        await _categoryRepository.DeleteAsync(id);
        return true;
    }

    private static void ValidateRequest(CreateCategoryRequest request)
        => ValidateName(request.Name);

    private static void ValidateRequest(UpdateCategoryRequest request)
        => ValidateName(request.Name);

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.");
        }
    }

    private static CategoryDto MapToDto(Category c)
    {
        return new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ParentCategoryId
        );
    }
}
