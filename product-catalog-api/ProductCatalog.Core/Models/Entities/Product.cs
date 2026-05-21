namespace ProductCatalog.Core.Models.Entities;

public class Product : IComparable<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Category? Category { get; set; }

    // IComparable: sort by Price ascending, then Name ascending
    public int CompareTo(Product? other)
    {
        if (other is null) return 1;

        int priceComparison = Price.CompareTo(other.Price);
        return priceComparison != 0
            ? priceComparison
            : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => $"[{Sku}] {Name} — ${Price:F2} ({Quantity} in stock)";
}
