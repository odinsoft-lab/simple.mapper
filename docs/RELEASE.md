# Release Notes

All release notes for Simple.AutoMapper, ordered from newest to oldest.

---

## v1.0.10

Release date: 2026-02-02

### Highlights

This release adds full Patch support with 4 overloads matching Map's signatures, replaces MapTo with Map in-place, and significantly improves documentation and test coverage.

### New Features

- **Patch New Object** - `Patch<TS, TD>(source)` creates new TD, skipping null source properties
- **Patch Type-Inferred** - `Patch<TD>(object source)` creates new TD with source type inferred at runtime
- **Patch Collection** - `Patch<TS, TD>(IEnumerable<TS>)` maps collection with null-skip semantics
- **Patch In-Place** - `Patch<TS, TD>(source, dest)` partial update, preserving destination values for null source properties

### API Changes

- **MapTo removed** from `ISimpleMapper` — replaced by `Map<TS, TD>(source, dest)` in-place overload
- `ISimpleMapper` now exposes **8 methods**: Map (4) + Patch (4)
- Static `Mapper` class also exposes the same 8 methods

### Documentation

- **Usage Guide** (`docs/GUIDE.md`) — Before/after comparisons for all 14 features
- Consolidated release notes into `docs/RELEASE.md`
- Consolidated roadmap into `docs/TASK.md`
- Simplified `README.md` with link to usage guide

### Samples

- Renamed `samples/BasicSample` → `samples/Console` with feature-split example files
- Renamed `samples/WebApiSample` → `samples/WebAPI` with PATCH endpoints added

### Test Coverage

- **92.9% line coverage**, 88.8% branch coverage (242 tests)
- Added `PatchTests.cs` (11 tests) for all Patch overloads and DI integration
- Added `CoverageBoostTests.cs` (32 tests) for targeted coverage improvements

### Breaking Changes

- `MapTo` methods removed from `ISimpleMapper`. Use `Map<TS, TD>(source, dest)` instead.

---

## v1.0.9

Release date: 2026-01-11

### Highlights

This release introduces powerful new mapping configuration options that bring Simple.AutoMapper closer to feature parity with full-featured mapping libraries, while maintaining simplicity and performance.

### New Features

- **Condition** - Conditional property mapping based on source values
- **NullSubstitute** - Default value substitution when source is null
- **BeforeMap / AfterMap** - Pre and post mapping callbacks
- **ConstructUsing** - Custom object construction for immutable types

### New Configuration APIs

#### Condition

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember(d => d.Email, opt => {
        opt.MapFrom(s => s.Email);
        opt.Condition(s => s.IsEmailVerified);
    });
```

#### NullSubstitute

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember(d => d.DisplayName, opt => {
        opt.MapFrom(s => s.Nickname);
        opt.NullSubstitute("Anonymous");
    });
```

#### BeforeMap / AfterMap

```csharp
Mapper.CreateMap<User, UserDto>()
    .BeforeMap((src, dest) => dest.CreatedAt = DateTime.UtcNow)
    .AfterMap((src, dest) => dest.FullName = $"{dest.FirstName} {dest.LastName}");
```

#### ConstructUsing

```csharp
Mapper.CreateMap<User, ImmutableUserDto>()
    .ConstructUsing(src => new ImmutableUserDto(src.Id, src.Name));
```

### Other Improvements

- **Test Coverage**: Increased from 63% to 87% line coverage
- **net10.0 Support**: Added .NET 10.0 target framework
- **Bug Fix**: Fixed PreserveReferences circular reference detection issue
- **Bug Fix**: Fixed net9.0 missing from DI support condition
- **Dependencies**: Upgraded Microsoft.Extensions.* packages to 10.0.1

### Breaking Changes

None. All existing v1.0.8 features remain compatible.

---

## v1.0.8

Release date: 2025-10-28

### Highlights

This release introduces comprehensive Dependency Injection (DI) support.

### New Features

- **ISimpleMapper Interface** - Abstraction for dependency injection
- **AddSimpleMapper() Extension** - One-line DI registration
- **MapperConfiguration** - Fluent configuration API
- **Profile Support** - Group related mappings together
- **Assembly Scanning** - Auto-discover profiles in assemblies

### DI Setup

```csharp
services.AddSimpleMapper(cfg => {
    cfg.AddProfile<UserMappingProfile>();
});

// Or assembly scanning
services.AddSimpleMapper(typeof(UserMappingProfile).Assembly);
```

### Target Frameworks for DI

- netstandard2.1, net8.0, net9.0 (netstandard2.0 excluded)

### Breaking Changes

None.

---

## v1.0.7

Release date: 2025-08-24

### Highlights

- PreserveReferences now strictly gates caching and circular-reference checks
- Depth tracking increments only when PreserveReferences is enabled
- Mapping engine reuses cached destination only for circular references when PreserveReferences is on

### Changes

- **MappingContext**: Bypass circular ref checks when PreserveReferences is false; increment depth only when enabled
- **MappingEngine**: Assign destination from cache on circular detection with PreserveReferences=true; clear compiled map cache on new CreateMap
- **Tests**: Added/updated circular reference tests

### Bug Fixes

- Eliminated stack overflows in circular graphs when PreserveReferences is enabled
- Prevented unintended instance reuse when PreserveReferences is disabled

### Behavior Notes

If your code previously relied on implicit instance reuse without explicitly enabling PreserveReferences, you must now opt in via `.PreserveReferences()`.

---

## v1.0.6

Release date: 2025-08-22

### Summary

Minor update focused on stability and developer experience.

### Key Changes

- NuGet package v1.0.6 published
- Enhanced XML documentation for MappingEngine/Mapper
- Added in-place update sample and test code
- Improved performance for cached mapping execution
- Updated and cleaned up documentation

### Notes

No breaking changes to the public API.

---

## v1.0.5

Release date: 2025-08-21

### Summary

Initial stable release. High-performance mapping library for .NET with expression tree compilation.

Targets: netstandard2.0, netstandard2.1, net8.0, net9.0

### Highlights

- Concise public API with internal compiled engine
- Collection mapping and list sync helper (`SyncResult`)
- Thread-safe caching, deterministic build, embedded PDB and symbol package
- ReverseMap support
- PreserveReferences / MaxDepth options (experimental)

### API Surface

- `Map<TSource, TDestination>(TSource)` - Single object
- `Map<TDestination>(object)` - Type-inferred
- `Map<TSource, TDestination>(IEnumerable<TSource>)` - Collection
- `Map<TSource, TDestination>(TSource, TDestination)` - In-place update
- `CreateMap<TSource, TDestination>()` - Configuration

### Known Limitations

- ForMember rules stored but not yet applied in compiled expressions
- Circular references not fully supported (PreserveReferences/MaxDepth are experimental)
- Destination types require a parameterless constructor (`new()`)
