# Simple.AutoMapper v1.0.5 — Release

Date: 2025-08-21

## Summary
Simple.AutoMapper is a high-performance mapping library for .NET. The public API centers on a simple, reflection-based Mapper, while internally a compiled MappingEngine compiles and caches mappings on first use to speed up subsequent calls. This release focuses on stability, multi-targeting, and documentation cleanup.

Targets: netstandard2.0, netstandard2.1, net8.0, net9.0

## Highlights
- Concise public API with an internal compiled engine working in tandem
- Collection mapping and list sync helper (`SyncResult`)
- Thread-safe caching, deterministic build, embedded PDB and symbol package (snupkg)
- ReverseMap support: automatically generates bi-directional map configuration ✅
- PreserveReferences / MaxDepth options (experimental) — cycle handling/depth control will be strengthened in future updates
- Documentation tidy-up (README, ROADMAP) and sample refresh: added in-place update example

## Breaking/Behavioral Notes
- ForMember configuration is currently captured but not applied in the compiled mapping yet.
- Cycle handling in v1.0.5 is not complete. PreserveReferences/MaxDepth are experimental and may not behave as expected for certain graphs.
- No public API signature changes in 1.0.x.

## API Surface
- Mapper (public, reflection-based)
  - `Map<TSource, TDestination>(TSource)`
  - `Map<TDestination>(object source)`
  - `Map<TSource, TDestination>(IEnumerable<TSource>)`
  - `Map<TSource, TDestination>(TSource source, TDestination destination)` — in-place update
  - List sync helper: returns upsert/remove results as `SyncResult`
- MappingEngine (internal, compiled)
  - `CreateMap<TSource, TDestination>()` configuration API (Ignore/ForMember application to be enhanced)

## Migration and Usage
For general usage, prefer the public Mapper API. For configuration-driven scenarios, define once using `CreateMap`, then invoke mapping through the public API.

```csharp
// Configuration example
Mapper.CreateMap<User, UserDto>();
Mapper.CreateMap<Address, AddressDto>();

// Single item / collection mapping
var dto = Mapper.Map<User, UserDto>(user);
var dtos = Mapper.Map<User, UserDto>(users);

// In-place update
Mapper.Map(source, existingDestination);
```

With EF Core, materialize first and then map.

```csharp
var users = await db.Users.AsNoTracking().Include(u => u.Address).ToListAsync();
var dtos = Mapper.Map<User, UserDto>(users);
```

### ReverseMap
```csharp
Mapper.CreateMap<User, UserDto>()
  .ReverseMap();
```

### PreserveReferences / MaxDepth (experimental)
```csharp
Mapper.CreateMap<Entity, EntityDto>()
  .PreserveReferences()  // Track circular references
  .MaxDepth(5);          // Limit maximum depth
```
Note: in v1.0.5, depth/reference tracking may be limited and is not guaranteed for deep graphs.

## Performance Notes
- On first call, mapping is compiled and cached; subsequent mappings of the same type pair run faster.
- Actual performance varies by environment; treat the first mapping as a warm-up.

## Known Limitations
- ForMember rules are stored but not yet applied in compiled expressions (planned). Ignore works.
- Circular references are not fully supported in v1.0.5 (risk of stack overflow). PreserveReferences/MaxDepth are experimental.
- Destination types require a parameterless constructor (`new()`).
- Do not call the mapping API inside IQueryable; it won't translate to SQL.

## Next
- Add a BenchmarkDotNet project and establish baseline benchmarks
- Complete application of ForMember (MapFrom/Condition, etc.) in compiled mappings
- DI integration and profile system refinement; add diagnostics/validation APIs

## Credits
Built by ODINSOFT. See README for team.
