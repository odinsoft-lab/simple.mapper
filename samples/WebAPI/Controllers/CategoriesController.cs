using Microsoft.AspNetCore.Mvc;
using Simple.AutoMapper.Interfaces;
using WebApiSample.Models;

namespace WebApiSample.Controllers;

/// <summary>
/// Categories REST API demonstrating ISimpleMapper (Dependency Injection pattern).
/// All mapping uses the injected ISimpleMapper instance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // In-memory storage for demonstration
    private static readonly List<CategoryEntity> _categories = new();
    private static int _nextId = 1;

    private readonly ILogger<CategoriesController> _logger;
    private readonly ISimpleMapper _mapper; // ISimpleMapper injected via DI

    public CategoriesController(
        ILogger<CategoriesController> logger,
        ISimpleMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// GET /api/categories
    /// Demonstrates ISimpleMapper.Map with collection.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<CategoryDto>> GetAll()
    {
        var categoryDtos = _mapper.Map<CategoryEntity, CategoryDto>(_categories);
        return Ok(categoryDtos);
    }

    /// <summary>
    /// GET /api/categories/{id}
    /// Demonstrates ISimpleMapper.Map with type-inferred single object.
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<CategoryDto> GetById(int id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = $"Category with ID {id} not found" });

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    /// <summary>
    /// POST /api/categories
    /// Demonstrates ISimpleMapper.Map for object creation.
    /// </summary>
    [HttpPost]
    public ActionResult<CategoryDto> Create([FromBody] CreateCategoryDto createDto)
    {
        var categoryEntity = _mapper.Map<CategoryEntity>(createDto);
        categoryEntity.Id = _nextId++;
        categoryEntity.CreatedAt = DateTime.UtcNow;
        categoryEntity.IsActive = true;

        _categories.Add(categoryEntity);

        var categoryDto = _mapper.Map<CategoryDto>(categoryEntity);
        return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
    }

    /// <summary>
    /// PUT /api/categories/{id}
    /// Demonstrates ISimpleMapper.Map for full in-place update.
    /// Map overwrites ALL matching properties on the destination.
    /// </summary>
    [HttpPut("{id}")]
    public ActionResult<CategoryDto> Update(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = $"Category with ID {id} not found" });

        // Map: overwrites ALL matching properties (null source → null/default destination)
        _mapper.Map(updateDto, category);
        category.UpdatedAt = DateTime.UtcNow;

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    /// <summary>
    /// PATCH /api/categories/{id}
    /// Demonstrates ISimpleMapper.Patch for partial update.
    /// Patch skips null source properties, preserving existing destination values.
    /// </summary>
    [HttpPatch("{id}")]
    public ActionResult<CategoryDto> PartialUpdate(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = $"Category with ID {id} not found" });

        // Patch: only non-null source properties are applied to destination
        _mapper.Patch(updateDto, category);
        category.UpdatedAt = DateTime.UtcNow;

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    /// <summary>
    /// DELETE /api/categories/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = $"Category with ID {id} not found" });

        _categories.Remove(category);
        return NoContent();
    }

    /// <summary>
    /// POST /api/categories/seed
    /// Seeds sample data for testing.
    /// </summary>
    [HttpPost("seed")]
    public ActionResult SeedData()
    {
        _categories.Clear();
        _nextId = 1;

        var sampleCategories = new List<CreateCategoryDto>
        {
            new() { Name = "Electronics", Description = "Electronic devices and accessories" },
            new() { Name = "Clothing", Description = "Apparel and fashion items" },
            new() { Name = "Books", Description = "Physical and digital books" },
            new() { Name = "Home & Garden", Description = "Home improvement and gardening supplies" },
            new() { Name = "Sports", Description = "Sports equipment and accessories" }
        };

        foreach (var dto in sampleCategories)
        {
            var entity = _mapper.Map<CategoryEntity>(dto);
            entity.Id = _nextId++;
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _categories.Add(entity);
        }

        return Ok(new { message = $"Seeded {sampleCategories.Count} categories", count = _categories.Count });
    }
}
