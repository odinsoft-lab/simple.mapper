# Simple.AutoMapper TODO

This document tracks future work items. Priorities and scope may change based on project needs.

Last updated: 2026-01-11

---

## Ready to Implement

### Configuration API Extensions

- [ ] **ConvertUsing** - Full type conversion control
  ```csharp
  CreateMap<Order, OrderDto>()
      .ConvertUsing(src => new OrderDto { ... });
  ```
  - Customize entire object conversion logic
  - Replaces entire mapping process (unlike ForMember)

- [ ] **TypeConverter** - Global type converters
  ```csharp
  Mapper.CreateTypeConverter<string, DateTime>(s => DateTime.Parse(s));
  Mapper.CreateTypeConverter<DateTime, string>(d => d.ToString("yyyy-MM-dd"));
  ```
  - Define type-to-type conversion rules globally
  - Automatically applied across all mappings

- [ ] **Rule-based Ignore/Include**
  ```csharp
  CreateMap<User, UserDto>()
      .IgnoreAllPropertiesWithAnInaccessibleSetter()
      .IgnoreAllSourcePropertiesWithAnInaccessibleGetter()
      .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
  ```

### Collections/Type Support Extensions

- [ ] **HashSet Mapping**
  ```csharp
  Mapper.Map<HashSet<User>, HashSet<UserDto>>(users);
  ```

- [ ] **Dictionary Mapping**
  ```csharp
  Mapper.Map<Dictionary<int, User>, Dictionary<int, UserDto>>(dict);
  ```

- [ ] **Record Type Support**
  - Auto-detect init-only properties
  - Constructor parameter mapping
  ```csharp
  public record UserDto(int Id, string Name);
  Mapper.Map<User, UserDto>(user); // Automatically uses constructor
  ```

- [ ] **ReadOnlyCollection Support**
  ```csharp
  Mapper.Map<List<User>, IReadOnlyList<UserDto>>(users);
  ```

- [ ] **Polymorphic Mapping**
  ```csharp
  CreateMap<Animal, AnimalDto>()
      .Include<Dog, DogDto>()
      .Include<Cat, CatDto>();
  ```

---

## Validation & Diagnostics

- [ ] **Configuration Validation API**
  ```csharp
  var config = Mapper.GetConfiguration();
  var result = config.AssertConfigurationIsValid();
  // Detects unmapped members, type mismatches, impossible conversions
  ```

- [ ] **Runtime Diagnostics**
  - Log level-based diagnostic information
  - EventSource integration
  - Mapping performance metrics

- [ ] **Unmapped Member Detection**
  ```csharp
  CreateMap<User, UserDto>()
      .ForAllOtherMembers(opt => opt.Ignore()); // Explicitly ignore remaining
  ```

---

## Performance Optimization

- [ ] **BenchmarkDotNet Integration**
  - Build micro-benchmark suite
  - Establish baseline performance metrics
  - Detect performance regressions in CI

- [ ] **Cache Improvements**
  - Provide warm-up API
  - Capacity/expiration policy
  - Cache metrics collection
  ```csharp
  Mapper.WarmUp<User, UserDto>();
  var stats = Mapper.GetCacheStatistics();
  ```

- [ ] **Allocation Reduction**
  - Streaming option for collection mapping
  - Investigate Span<T> input support
  - Pooling strategies

---

## Integration & Platform

- [ ] **XML Doc Comments Cleanup**
  - Add documentation to all public APIs
  - Include in NuGet package

- [ ] **SourceLink Integration**
  - Enable source navigation during debugging
  - Optimize symbol packages (.snupkg)

- [ ] **GitHub Actions CI/CD**
  - Automated build/test
  - Automated package creation
  - Release draft automation
  ```yaml
  # .github/workflows/ci.yml
  on: [push, pull_request]
  jobs:
    build:
      - dotnet build
      - dotnet test
      - dotnet pack
  ```

---

## Long-term Roadmap

### IQueryable Projections

Integrate with EF Core for database-level projections:

```csharp
var dtos = await db.Users
    .ProjectTo<UserDto>(Mapper.Configuration)
    .ToListAsync();
// SELECT Id, Name FROM Users (only required columns)
```

Implementation requirements:
- Generate Expression Trees translatable by EF Core
- Detect unsupported features and provide fallback

### Source Generator

Generate mapper code at compile time:

```csharp
[GenerateMapper]
public partial class UserMapper : IMapper<User, UserDto> { }
```

Benefits:
- Eliminate runtime reflection
- AOT compilation support
- Improved startup time

### Rule-based Naming/Flattening

```csharp
CreateMap<Order, OrderDto>()
    .EnableFlattening(); // Order.Customer.Name -> OrderCustomerName

CreateMap<OrderDto, Order>()
    .EnableUnflattening(); // OrderCustomerName -> Order.Customer.Name
```

### Roslyn Analyzer Package

```csharp
// SM001: Unmapped destination member 'Email'
CreateMap<User, UserDto>(); // Warning

// SM002: Possible null reference in MapFrom
opt.MapFrom(s => s.Address.City); // Warning if Address can be null
```

### Documentation Site

- DocFX or Docusaurus based
- Auto-generated API Reference
- Sample gallery
- Search functionality

---

## Bug Fixes & Improvements

- [ ] **MaxDepth Full Implementation**
  - Currently limited enforcement
  - Test all scenarios

- [ ] **Nullable Reference Types Support**
  - Enable `<Nullable>enable</Nullable>`
  - Resolve warnings

- [x] **net9.0 DI Condition Fix** (2026-01-11)
  - Fixed missing net9.0 in DI condition on `Simple.Mapper.csproj` line 88
  - Simplified to `Condition="'$(TargetFramework)' != 'netstandard2.0'"`

---

## Completed Items

### v1.0.9 (2026-01-11)
- [x] ForMember Condition
- [x] ForMember NullSubstitute
- [x] BeforeMap/AfterMap
- [x] ConstructUsing
- [x] net10.0 support
- [x] Test coverage 87%

### v1.0.8 (2025-10-28)
- [x] ISimpleMapper interface
- [x] AddSimpleMapper() DI extension
- [x] MapperConfiguration
- [x] Profile support
- [x] Assembly scanning

### v1.0.7 (2025-08-24)
- [x] PreserveReferences improvements
- [x] Circular reference handling
- [x] MaxDepth basic implementation

---

## Contribution Guide

Before starting work:
1. Create an issue to share your intent
2. Write related tests
3. Verify existing tests pass (`dotnet test`)
4. Maintain code coverage at 85% or higher

PR Checklist:
- [ ] Add/modify tests
- [ ] Add XML doc comments (public API)
- [ ] Update CHANGELOG
- [ ] No breaking changes (or documented)
