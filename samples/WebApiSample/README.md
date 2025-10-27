# Simple.AutoMapper Web API Sample

This sample demonstrates how to use **Simple.AutoMapper** with Dependency Injection (DI) pattern in an ASP.NET Core Web API application.

## Features Demonstrated

1. **DI Integration** - Using `AddSimpleMapper()` extension method
2. **Profile Pattern** - Organizing mappings with `Profile` base class
3. **Entity to DTO Mapping** - Converting database models to API responses
4. **DTO to Entity Mapping** - Creating entities from API requests
5. **List Mapping** - Converting collections
6. **In-Place Updates** - Updating existing objects with `Mapper.Update()`

## Project Structure

```
WebApiSample/
├── Controllers/
│   └── ProductsController.cs    # REST API endpoints
├── Models/
│   ├── ProductEntity.cs         # Database entity model
│   └── ProductDto.cs            # API DTO models
├── Profiles/
│   └── ProductProfile.cs        # Mapping configuration
└── Program.cs                   # DI setup with AddSimpleMapper
```

## Key Code Snippets

### 1. DI Configuration (Program.cs)

```csharp
using Simple.AutoMapper.DependencyInjection;
using WebApiSample.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Configure Simple.AutoMapper with DI pattern
builder.Services.AddSimpleMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
});
```

### 2. Profile Configuration (ProductProfile.cs)

```csharp
public class ProductProfile : Profile
{
    protected override void Configure()
    {
        // Entity ↔ DTO bidirectional mapping
        CreateMap<ProductEntity, ProductDto>().ReverseMap();

        // Request DTO → Entity with custom values
        CreateMap<CreateProductDto, ProductEntity>()
            .ForMember(dest => dest.CreatedAt, _ => DateTime.UtcNow)
            .ForMember(dest => dest.IsActive, _ => true);
    }
}
```

### 3. Controller Usage (ProductsController.cs)

```csharp
// Single object mapping
var productDto = Mapper.Map<ProductDto>(productEntity);

// Collection mapping
var productDtos = Mapper.Map<List<ProductDto>>(productEntities);

// In-place update
Mapper.Update(updateDto, existingEntity);
```

## Running the Sample

1. **Build the project**:
   ```bash
   cd samples/WebApiSample
   dotnet build
   ```

2. **Run the API**:
   ```bash
   dotnet run
   ```

3. **Test the endpoints**:

   The API will be available at `http://localhost:5000` (or the port shown in console)

   ```bash
   # Seed sample data
   curl -X POST http://localhost:5000/api/products/seed

   # Get all products
   curl http://localhost:5000/api/products

   # Get product by ID
   curl http://localhost:5000/api/products/1

   # Create new product
   curl -X POST http://localhost:5000/api/products \
     -H "Content-Type: application/json" \
     -d '{"name":"Tablet","description":"10-inch tablet","price":399.99,"stock":20}'

   # Update product
   curl -X PUT http://localhost:5000/api/products/1 \
     -H "Content-Type: application/json" \
     -d '{"price":1199.99,"stock":15}'

   # Delete product
   curl -X DELETE http://localhost:5000/api/products/1
   ```

4. **OpenAPI/Swagger** (Development mode only):
   - Navigate to `http://localhost:5000/openapi/v1.json` for OpenAPI specification

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update existing product |
| DELETE | `/api/products/{id}` | Delete product |
| POST | `/api/products/seed` | Seed sample data |

## Mapping Examples

### Entity → DTO (API Response)
```csharp
var entity = new ProductEntity
{
    Id = 1,
    Name = "Laptop",
    InternalNotes = "Secret data"  // This field won't be mapped
};

var dto = Mapper.Map<ProductDto>(entity);
// dto.InternalNotes doesn't exist in ProductDto
```

### Request DTO → Entity (Create)
```csharp
var createDto = new CreateProductDto
{
    Name = "Mouse",
    Price = 29.99m
};

var entity = Mapper.Map<ProductEntity>(createDto);
// entity.CreatedAt is auto-set to DateTime.UtcNow
// entity.IsActive is auto-set to true
```

### Update DTO → Entity (Update)
```csharp
var updateDto = new UpdateProductDto
{
    Price = 24.99m  // Only price is set
};

Mapper.Update(updateDto, existingEntity);
// Only Price property is updated
// Other properties remain unchanged
```

## Advantages of DI Pattern

1. **ASP.NET Core Integration** - Consistent with framework conventions
2. **Testability** - Easy to mock and test
3. **Centralized Configuration** - All mappings configured at startup
4. **Type Safety** - Compile-time checking of Profile types

## Learn More

- [Simple.AutoMapper Documentation](https://github.com/odinsoft-lab/simple.mapper)
- [Profile Pattern](https://github.com/odinsoft-lab/simple.mapper#profile-pattern)
- [DI Integration](https://github.com/odinsoft-lab/simple.mapper#dependency-injection)
