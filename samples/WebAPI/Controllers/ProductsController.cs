using Microsoft.AspNetCore.Mvc;
using Simple.AutoMapper.Core;
using WebApiSample.Models;

namespace WebApiSample.Controllers;

/// <summary>
/// Products REST API demonstrating static Mapper facade (no DI).
/// All mapping uses the static Mapper class directly.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // In-memory storage for demonstration
    private static readonly List<ProductEntity> _products = new();
    private static int _nextId = 1;

    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /api/products
    /// Demonstrates Mapper.Map with type-inferred collection.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        var productDtos = Mapper.Map<List<ProductDto>>(_products);
        return Ok(productDtos);
    }

    /// <summary>
    /// GET /api/products/{id}
    /// Demonstrates Mapper.Map with type-inferred single object.
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<ProductDto> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        var productDto = Mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    /// <summary>
    /// POST /api/products
    /// Demonstrates Mapper.Map for object creation.
    /// </summary>
    [HttpPost]
    public ActionResult<ProductDto> Create([FromBody] CreateProductDto createDto)
    {
        var productEntity = Mapper.Map<ProductEntity>(createDto);
        productEntity.Id = _nextId++;
        productEntity.CreatedAt = DateTime.UtcNow;
        productEntity.IsActive = true;
        productEntity.Version = 1;

        _products.Add(productEntity);

        var productDto = Mapper.Map<ProductDto>(productEntity);
        return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
    }

    /// <summary>
    /// PUT /api/products/{id}
    /// Demonstrates Mapper.Map for full in-place update.
    /// Map overwrites ALL matching properties on the destination.
    /// </summary>
    [HttpPut("{id}")]
    public ActionResult<ProductDto> Update(int id, [FromBody] UpdateProductDto updateDto)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        // Map: overwrites ALL properties (null source → null/default destination)
        Mapper.Map(updateDto, product);
        product.UpdatedAt = DateTime.UtcNow;

        var productDto = Mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    /// <summary>
    /// PATCH /api/products/{id}
    /// Demonstrates Mapper.Patch for partial update.
    /// Patch skips null source properties, preserving existing destination values.
    /// </summary>
    [HttpPatch("{id}")]
    public ActionResult<ProductDto> PartialUpdate(int id, [FromBody] UpdateProductDto updateDto)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        // Patch: only non-null source properties are applied
        Mapper.Patch(updateDto, product);
        product.UpdatedAt = DateTime.UtcNow;

        var productDto = Mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    /// <summary>
    /// DELETE /api/products/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        _products.Remove(product);
        return NoContent();
    }

    /// <summary>
    /// POST /api/products/seed
    /// Seeds sample data for testing.
    /// </summary>
    [HttpPost("seed")]
    public ActionResult SeedData()
    {
        _products.Clear();
        _nextId = 1;

        var sampleProducts = new List<CreateProductDto>
        {
            new() { Name = "Laptop", Description = "High-performance laptop", Price = 1299.99m, Stock = 10 },
            new() { Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 50 },
            new() { Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Stock = 25 },
            new() { Name = "Monitor", Description = "27-inch 4K monitor", Price = 449.99m, Stock = 15 },
            new() { Name = "Headphones", Description = "Noise-canceling headphones", Price = 199.99m, Stock = 30 }
        };

        foreach (var dto in sampleProducts)
        {
            var entity = Mapper.Map<ProductEntity>(dto);
            entity.Id = _nextId++;
            _products.Add(entity);
        }

        return Ok(new { message = $"Seeded {sampleProducts.Count} products", count = _products.Count });
    }
}
