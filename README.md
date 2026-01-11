# Simple.AutoMapper

[![NuGet](https://img.shields.io/nuget/v/Simple.AutoMapper.svg)](https://www.nuget.org/packages/Simple.AutoMapper/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Downloads](https://img.shields.io/nuget/dt/Simple.AutoMapper.svg)](https://www.nuget.org/packages/Simple.AutoMapper/)


High-performance object mapping for .NET with expression tree compilation. Simple API, powerful configuration options.

> **v1.0.9** - Added Condition, NullSubstitute, BeforeMap/AfterMap, ConstructUsing. See [Release Notes](docs/releases/v1.0.9.md).

## Installation

```powershell
dotnet add package Simple.AutoMapper
```

**Target Frameworks:** netstandard2.0, netstandard2.1, net8.0, net9.0, net10.0

## Quick Start

```csharp
using Simple.AutoMapper.Core;

// Simple mapping - no configuration needed
var dto = Mapper.Map<User, UserDto>(user);
var dtos = Mapper.Map<User, UserDto>(users);

// In-place update
Mapper.Map(source, existingDestination);
```

## Configuration Options

### Basic Configuration

```csharp
// Configure mapping with options
Mapper.CreateMap<User, UserDto>()
    .Ignore(d => d.Password)
    .ReverseMap();  // Creates UserDto -> User mapping too
```

### ForMember with MapFrom

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
    .ForMember(d => d.Age, opt => opt.MapFrom(s => DateTime.Now.Year - s.BirthYear));
```

### Condition - Conditional Mapping

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember(d => d.Email, opt => {
        opt.MapFrom(s => s.Email);
        opt.Condition(s => s.IsEmailVerified);  // Only map if condition is true
    });
```

### NullSubstitute - Default Values

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember(d => d.DisplayName, opt => {
        opt.MapFrom(s => s.Nickname);
        opt.NullSubstitute("Anonymous");  // Use "Anonymous" if source is null
    });
```

### BeforeMap / AfterMap - Callbacks

```csharp
Mapper.CreateMap<User, UserDto>()
    .BeforeMap((src, dest) => {
        // Execute before property mapping
        dest.MappedAt = DateTime.UtcNow;
    })
    .AfterMap((src, dest) => {
        // Execute after property mapping
        dest.FullName = $"{dest.FirstName} {dest.LastName}";
    });
```

### ConstructUsing - Custom Construction

```csharp
// For types without parameterless constructor
Mapper.CreateMap<User, UserDto>()
    .ConstructUsing(src => new UserDto(src.Id));

// For immutable objects
Mapper.CreateMap<User, ImmutableUserDto>()
    .ConstructUsing(src => new ImmutableUserDto(
        src.Id,
        src.Name,
        src.Email
    ));
```

### Circular Reference Handling

```csharp
Mapper.CreateMap<Parent, ParentDto>()
    .PreserveReferences()  // Handle circular references
    .MaxDepth(5);          // Limit recursion depth
```

## Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddSimpleMapper(cfg => {
    cfg.AddProfile<UserMappingProfile>();
});

// Or scan assemblies
services.AddSimpleMapper(typeof(UserMappingProfile).Assembly);
```

### Creating Profiles

```csharp
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"));

        CreateMap<Address, AddressDto>();
    }
}
```

## EF Core Integration

### Read Operations

```csharp
// Materialize first, then map
var users = await db.Users
    .AsNoTracking()
    .Include(u => u.Address)
    .ToListAsync();

var dtos = Mapper.Map<User, UserDto>(users);
```

### Write Operations

```csharp
// Create
var entity = Mapper.Map<CreateUserDto, User>(dto);
await db.Users.AddAsync(entity);

// Update in-place
var entity = await db.Users.FindAsync(id);
Mapper.Map(dto, entity);
await db.SaveChangesAsync();
```

### Collection Sync

```csharp
var result = Mapper.Map(
    dtoList,
    entityList,
    dto => dto.Id,
    entity => entity.Id,
    removeMissing: true
);
// result.Added, result.Updated, result.Removed
```

## Type Support

| Type | Support |
|------|---------|
| Simple types (int, string, DateTime, Guid, etc.) | ✅ |
| Nullable types | ✅ |
| Enums | ✅ |
| Complex types (classes) | ✅ Recursive |
| Collections (List, Array, IEnumerable) | ✅ |
| Circular references | ✅ With PreserveReferences() |

## Performance

- Expression tree compilation for fast subsequent mappings
- Thread-safe caching of compiled mappers
- First mapping incurs compilation cost; subsequent calls are optimized

## Samples & Tests

- See `samples/` for end-to-end usage examples
- See `tests/` for comprehensive test coverage including edge cases

## License

MIT License - see [LICENSE.md](LICENSE.md)

## Team

**Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect
- **YUJIN** - Senior Developer & Exchange Integration Specialist
- **SEJIN** - Software Developer & API Implementation

---

**Built with care by the ODINSOFT Team** | [GitHub](https://github.com/odinsoft-lab/simple.mapper)
