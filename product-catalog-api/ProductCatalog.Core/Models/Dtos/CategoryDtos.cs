using System.Text.Json.Serialization;

namespace ProductCatalog.Core.Models.Dtos;

// --- Read DTOs ---

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId
);

[JsonConverter(typeof(CategoryTreeNodeJsonConverter))]
public record CategoryTreeNode(
    int Id,
    string Name,
    string Description,
    List<CategoryTreeNode> Children
);

// --- Write DTOs ---

public record CreateCategoryRequest(
    string Name,
    string Description,
    int? ParentCategoryId
);

public record UpdateCategoryRequest(
    string Name,
    string Description,
    int? ParentCategoryId
);
