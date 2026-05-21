using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Core.Models.Dtos;
using ProductCatalog.Core.Services;
using System.Text.Json;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
        [FromQuery] ProductSearchFilters? filters)
    {
        var result = await _productService.GetAllAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        return product is null
            ? NotFound(new { Message = $"Product with ID {id} not found." })
            : Ok(product);
    }

    /// <summary>
    /// Demonstrates manual model binding — reads from Request.Body directly
    /// instead of relying on [FromBody] model binding.
    /// </summary>
    [HttpPost("manual")]
    public async Task<ActionResult<ProductDto>> CreateProductManual()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        CreateProductRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CreateProductRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            return BadRequest(new { Message = "Invalid JSON payload.", Detail = ex.Message });
        }

        if (request is null)
        {
            return BadRequest(new { Message = "Request body is empty." });
        }

        // Manual validation with pattern matching
        if (request is not { Name: not null and not "", Sku: not null and not "", Price: > 0 })
        {
            return BadRequest(new { Message = "Invalid product data. Name, SKU required. Price must be > 0." });
        }

        try
        {
            var created = await _productService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = created.Id },
                created
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var created = await _productService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetProduct),
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
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var updated = await _productService.UpdateAsync(id, request);

            return updated is null
                ? NotFound(new { Message = $"Product with ID {id} not found." })
                : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var deleted = await _productService.DeleteAsync(id);

        return !deleted
            ? NotFound(new { Message = $"Product with ID {id} not found." })
            : NoContent();
    }
}
