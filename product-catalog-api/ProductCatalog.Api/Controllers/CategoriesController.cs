using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Core.Models.Dtos;
using ProductCatalog.Core.Services;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Returns hierarchical category tree with custom JSON serialization.
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeNode>>> GetCategoryTree()
    {
        var tree = await _categoryService.GetTreeAsync();
        return Ok(tree);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        return category is null
            ? NotFound(new { Message = $"Category with ID {id} not found." })
            : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var created = await _categoryService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetCategory),
                new { id = created.Id },
                created
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var updated = await _categoryService.UpdateAsync(id, request);

            return updated is null
                ? NotFound(new { Message = $"Category with ID {id} not found." })
                : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);

        return !deleted
            ? NotFound(new { Message = $"Category with ID {id} not found." })
            : NoContent();
    }
}
