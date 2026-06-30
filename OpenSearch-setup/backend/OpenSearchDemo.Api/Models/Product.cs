namespace OpenSearchDemo.Api.Models;

/// <summary>
/// Product document model for OpenSearch indexing.
/// </summary>
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public double Rating { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ProductSpecifications? Specifications { get; set; }
}

public class ProductSpecifications
{
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Color { get; set; }
}
