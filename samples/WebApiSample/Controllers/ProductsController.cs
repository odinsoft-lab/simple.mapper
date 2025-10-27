using Microsoft.AspNetCore.Mvc;
using Simple.AutoMapper.Core;
using WebApiSample.Models;

namespace WebApiSample.Controllers;

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
    /// Get all products
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<ProductDto>> GetAll()
    {
        _logger.LogInformation("Getting all products");

        // Map List<ProductEntity> to List<ProductDto>
        var productDtos = Mapper.Map<List<ProductDto>>(_products);

        return Ok(productDtos);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<ProductDto> GetById(int id)
    {
        _logger.LogInformation("Getting product with ID: {Id}", id);

        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        // Map ProductEntity to ProductDto
        var productDto = Mapper.Map<ProductDto>(product);

        return Ok(productDto);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    public ActionResult<ProductDto> Create([FromBody] CreateProductDto createDto)
    {
        _logger.LogInformation("Creating new product: {Name}", createDto.Name);

        // Map CreateProductDto to ProductEntity
        var productEntity = Mapper.Map<ProductEntity>(createDto);
        productEntity.Id = _nextId++;
        productEntity.CreatedAt = DateTime.UtcNow;
        productEntity.IsActive = true;
        productEntity.Version = 1;

        _products.Add(productEntity);

        // Map ProductEntity back to ProductDto for response
        var productDto = Mapper.Map<ProductDto>(productEntity);

        return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("{id}")]
    public ActionResult<ProductDto> Update(int id, [FromBody] UpdateProductDto updateDto)
    {
        _logger.LogInformation("Updating product with ID: {Id}", id);

        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        // Manually update non-null properties
        if (updateDto.Name != null) product.Name = updateDto.Name;
        if (updateDto.Description != null) product.Description = updateDto.Description;
        if (updateDto.Price.HasValue) product.Price = updateDto.Price.Value;
        if (updateDto.Stock.HasValue) product.Stock = updateDto.Stock.Value;
        if (updateDto.IsActive.HasValue) product.IsActive = updateDto.IsActive.Value;
        product.UpdatedAt = DateTime.UtcNow;

        // Map updated entity to DTO
        var productDto = Mapper.Map<ProductDto>(product);

        return Ok(productDto);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        _logger.LogInformation("Deleting product with ID: {Id}", id);

        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        _products.Remove(product);

        return NoContent();
    }

    /// <summary>
    /// Seed sample data
    /// </summary>
    [HttpPost("seed")]
    public ActionResult SeedData()
    {
        _logger.LogInformation("Seeding sample data");

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
