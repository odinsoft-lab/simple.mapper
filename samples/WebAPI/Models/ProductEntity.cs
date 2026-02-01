namespace WebApiSample.Models;

/// <summary>
/// Product entity representing database model
/// </summary>
public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    // Internal fields not exposed to API
    public string InternalNotes { get; set; } = string.Empty;
    public int Version { get; set; }
}
