# Simple.AutoMapper

High-performance object mapping for .NET. Public usage focuses on a simple reflection-based Mapper API. A compiled MappingEngine exists for internal use and configuration experiments.

Note: This README reflects v1.0.5. See docs/RELEASE.md for details.

## Installation

- NuGet package: Simple.AutoMapper
- Target Frameworks: netstandard2.0, netstandard2.1, net8.0, net9.0

```powershell
# PowerShell (Windows)
dotnet add package Simple.AutoMapper
```

## What’s inside

There are two components:

1) Mapper (simple, zero-config, reflection-based) — Public API
- `Map<TSource, TDestination>(TSource)`
- `Map<TDestination>(object source)`
- `Map<TSource, TDestination>(IEnumerable<TSource>)`
- `Map<TSource, TDestination>(TSource source, TDestination destination)` for in-place updates
- List sync helpers returning SyncResult for upsert/remove operations

2) MappingEngine (compiled, configurable) — Internal mapping methods
- `CreateMap<TSource, TDestination>()` for future configuration support (e.g., Ignore/ForMember capture)
- Compiled mapping methods are internal and not intended to be called from application code

## Quick start — Mapping configuration (optional)

```csharp
using Simple.AutoMapper.Core;

// 1) Configure once at startup
Mapper.CreateMap<UserEntity, UserDTO>();
Mapper.CreateMap<AddressEntity, AddressDTO>();

// 2) Map anywhere in your code using the public Mapper API
var userDto = Mapper.Map<UserEntity, UserDTO>(userEntity);
var userDtos = Mapper.Map<UserEntity, UserDTO>(userEntities);
```

Configuration helpers:

```csharp
Mapper.CreateMap<Entity1, EntityDTO1>()
  .Ignore(d => d.SomeProperty)
  // Experimental in v1.0.5 — not fully supported, may not prevent cycles
  // .PreserveReferences()
  // .MaxDepth(5)

// Bidirectional mapping
Mapper.CreateMap<User, UserDto>()
  .ReverseMap();        // Auto-create UserDto -> User mapping

// NOTE: ForMember is currently stored but not yet applied at compile-time.
// NOTE: PreserveReferences/MaxDepth are experimental in v1.0.5 and can be considered placeholders.
```

EF Core read example (materialize first, then map):

```csharp
// Configure once (e.g., at startup) and keep engine as a singleton
Mapper.CreateMap<User, UserDto>();
Mapper.CreateMap<Address, AddressDto>();

// Read list
var users = await db.Users
  .AsNoTracking()
  .Include(u => u.Address)
  .ToListAsync(cancellationToken); // materialize here

var dtos = Mapper.Map<User, UserDto>(users);

// Read single
var user = await db.Users
  .AsNoTracking()
  .Include(u => u.Address)
  .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

var dto = Mapper.Map<User, UserDto>(user);

// Pagination pattern
var query = db.Users.AsNoTracking();
var total = await query.CountAsync();
var page = await query.OrderBy(u => u.Id).Skip((i - 1) * size).Take(size).ToListAsync();
var pageDtos = Mapper.Map<User, UserDto>(page);
```

## Quick start — Mapper (no configuration)

```csharp
using Simple.AutoMapper.Core;

// Single object
var dto = Mapper.Map<Entity8, EntityDTO8>(entity);

// Collection
var dtos = Mapper.Map<Entity17, EntityDTO17>(entities);

// In-place update
Mapper.Map(sourceEntity, existingDto);

// List synchronization with keys
var result = Mapper.Map(dtoList, entityList, dto => dto.Id, e => e.Id, removeMissing: true);
// result: SyncResult { Added, Updated, Removed }
```

EF Core write/update patterns:

```csharp
// Create (DTO -> Entity)
var order = Mapper.Map<CreateOrderDto, Order>(dto);
await db.Orders.AddAsync(order, cancellationToken);
await db.SaveChangesAsync(cancellationToken);

// Update in-place (DTO -> tracked Entity)
var entity = await db.Orders
  .Include(o => o.Items)
  .FirstOrDefaultAsync(o => o.Id == dto.Id, cancellationToken);
if (entity == null) return NotFound();

Mapper.Map(dto, entity);            // copy fields into the tracked instance
// Protect fields manually if needed (e.g., Id, RowVersion)
await db.SaveChangesAsync(cancellationToken);

// Child collection sync (key-based upsert/remove)
var sync = Mapper.Map(
  dto.Items,
  entity.Items,
  d => d.Id,
  e => e.Id,
  removeMissing: true);
// sync.Added / Updated / Removed
await db.SaveChangesAsync(cancellationToken);
```

## Capabilities

- Simple types, nullable simple types, enums, string, DateTime, Guid
- Complex types (class) are mapped recursively
 - Collections: `List<T>` and arrays, plus common `IEnumerable`/`ICollection`/`IList` shapes
- Null-safe: null sources or members remain null on destination
- Thread-safe caching for MappingEngine

## Performance notes

- MappingEngine compiles on first use and reuses cached delegates — subsequent calls are typically faster.
- Actual timings depend on environment; treat the first mapping as a warm-up.

## Limitations (current)

- ForMember mappings are captured but not yet emitted into the compiled expression; Ignore works.
- Circular references aren’t handled in v1.0.5 (may cause stack overflows for cyclic graphs). The PreserveReferences/MaxDepth options are experimental and not fully supported.
- Destination types must have parameterless constructors (new()).
- Do not call mapping APIs inside IQueryable; they cannot be translated to SQL. Use EF Core Select for projections and map after materialization.

## Samples and tests

- See samples/Program.cs and samples/MappingEngineExample.cs for end-to-end usage.
- Extensive unit tests live under tests/ covering nested objects, collections, nullables, and performance characteristics.

## License

See LICENSE.md.

## 👥 Team

### **Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect ([lisa@odinsoft.co.kr](mailto:lisa@odinsoft.co.kr))
- **YUJIN** - Senior Developer & Exchange Integration Specialist ([yoojin@odinsoft.co.kr](mailto:yoojin@odinsoft.co.kr))
- **SEJIN** - Software Developer & API Implementation ([saejin@odinsoft.co.kr](mailto:saejin@odinsoft.co.kr))

---

**Built with ❤️ by the ODINSOFT Team**