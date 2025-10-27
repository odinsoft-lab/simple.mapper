using Microsoft.AspNetCore.Mvc;
using Simple.AutoMapper.Interfaces;
using WebApiSample.Models;

namespace WebApiSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // In-memory storage for demonstration
    private static readonly List<CategoryEntity> _categories = new();
    private static int _nextId = 1;

    private readonly ILogger<CategoriesController> _logger;
    private readonly ISimpleMapper _mapper; // ISimpleMapper injected via DI

    /// <summary>
    /// Constructor with ISimpleMapper dependency injection
    /// </summary>
    public CategoriesController(
        ILogger<CategoriesController> logger,
        ISimpleMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all categories (demonstrates ISimpleMapper.Map with List)
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<CategoryDto>> GetAll()
    {
        _logger.LogInformation("Getting all categories using injected ISimpleMapper");

        // Use injected ISimpleMapper to map List
        var categoryDtos = _mapper.Map<CategoryEntity, CategoryDto>(_categories);

        return Ok(categoryDtos);
    }

    /// <summary>
    /// Get category by ID (demonstrates ISimpleMapper.Map single object)
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<CategoryDto> GetById(int id)
    {
        _logger.LogInformation("Getting category with ID: {Id} using injected ISimpleMapper", id);

        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        // Use injected ISimpleMapper to map single object
        var categoryDto = _mapper.Map<CategoryDto>(category);

        return Ok(categoryDto);
    }

    /// <summary>
    /// Create new category (demonstrates ISimpleMapper.Map for creation)
    /// </summary>
    [HttpPost]
    public ActionResult<CategoryDto> Create([FromBody] CreateCategoryDto createDto)
    {
        _logger.LogInformation("Creating new category: {Name} using injected ISimpleMapper", createDto.Name);

        // Use injected ISimpleMapper to map CreateCategoryDto to Entity
        var categoryEntity = _mapper.Map<CategoryEntity>(createDto);
        categoryEntity.Id = _nextId++;
        categoryEntity.CreatedAt = DateTime.UtcNow;
        categoryEntity.IsActive = true;

        _categories.Add(categoryEntity);

        // Map back to DTO for response
        var categoryDto = _mapper.Map<CategoryDto>(categoryEntity);

        return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
    }

    /// <summary>
    /// Update existing category (demonstrates ISimpleMapper.MapTo for in-place update)
    /// </summary>
    [HttpPut("{id}")]
    public ActionResult<CategoryDto> Update(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        _logger.LogInformation("Updating category with ID: {Id} using injected ISimpleMapper", id);

        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        // Use injected ISimpleMapper to update existing entity in-place
        _mapper.MapTo(updateDto, category);
        category.UpdatedAt = DateTime.UtcNow;

        // Map updated entity to DTO
        var categoryDto = _mapper.Map<CategoryDto>(category);

        return Ok(categoryDto);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        _logger.LogInformation("Deleting category with ID: {Id}", id);

        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        _categories.Remove(category);

        return NoContent();
    }

    /// <summary>
    /// Seed sample categories
    /// </summary>
    [HttpPost("seed")]
    public ActionResult SeedData()
    {
        _logger.LogInformation("Seeding sample categories using injected ISimpleMapper");

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
