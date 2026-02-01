# Tasks & Roadmap

This document consolidates the project roadmap, TODO items, and short-term tasks.

Last updated: 2026-02-02

---

## Completed Features

### v1.0.10 (in progress)

- [x] Patch 4 overloads: new object, type-inferred, collection, in-place (null-skip semantics)
- [x] Remove MapTo from ISimpleMapper, replace with Map in-place overload
- [x] ISimpleMapper: 8 methods (Map 4 + Patch 4)
- [x] Test coverage increased to 92.9% line, 88.8% branch (242 tests)
- [x] CoverageBoostTests.cs: 32 targeted coverage tests
- [x] PatchTests.cs: 11 Patch overload + DI tests
- [x] docs/GUIDE.md: Before/after usage guide with API reference
- [x] Samples restructured: BasicSample → Console, WebApiSample → WebAPI
- [x] Sample code split into feature files (Models, Map, Patch, Configuration, Comparison)
- [x] PATCH endpoints added to both WebAPI controllers
- [x] Consolidated docs: RELEASE.md, TASK.md, DEPLOYMENT.md
- [x] README.md simplified with link to usage guide

### v1.0.9 (2026-01-11)

- [x] ForMember Condition (conditional mapping)
- [x] ForMember NullSubstitute (null value replacement)
- [x] BeforeMap/AfterMap hooks
- [x] ConstructUsing (custom object construction)
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

## Ready to Implement

### Configuration API Extensions

- [ ] **ConvertUsing** - Full type conversion control
  ```csharp
  CreateMap<Order, OrderDto>()
      .ConvertUsing(src => new OrderDto { ... });
  ```

- [ ] **TypeConverter** - Global type converters
  ```csharp
  Mapper.CreateTypeConverter<string, DateTime>(s => DateTime.Parse(s));
  ```

- [ ] **Rule-based Ignore/Include**
  ```csharp
  CreateMap<User, UserDto>()
      .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
  ```

### Collections/Type Support

- [ ] **HashSet Mapping**
- [ ] **Dictionary Mapping**
- [ ] **Record Type Support** (auto-detect init-only, constructor parameter mapping)
- [ ] **ReadOnlyCollection Support**
- [ ] **Polymorphic Mapping** (Include/IncludeBase)

---

## Validation & Diagnostics

- [ ] **Configuration Validation API** (`AssertConfigurationIsValid()`)
- [ ] **Runtime Diagnostics** (log levels, EventSource, performance metrics)
- [ ] **Unmapped Member Detection** (`ForAllOtherMembers`)

---

## Performance Optimization

- [ ] **BenchmarkDotNet Integration** (micro-benchmark suite, baseline, CI regression detection)
- [ ] **Cache Improvements** (warm-up API, capacity/expiration policy, metrics)
- [ ] **Allocation Reduction** (streaming collection mapping, Span<T>, pooling)

---

## Integration & Platform

- [ ] **SourceLink Integration** (source navigation during debugging)
- [ ] **GitHub Actions CI/CD** (automated build/test/pack/release)

---

## Long-term Roadmap (3-6 months+)

- [ ] **IQueryable Projections** (expression trees for EF Core)
- [ ] **Source Generator** (compile-time mapper generation, AOT support)
- [ ] **Rule-based Naming/Flattening** (`Parent.ChildId` <-> `ParentChildId`)
- [ ] **Roslyn Analyzer Package** (unmapped members, null reference warnings)
- [ ] **Documentation Site** (DocFX/Docusaurus)

---

## Bug Fixes & Improvements

- [ ] **MaxDepth Full Implementation** (currently limited enforcement)
- [ ] **Nullable Reference Types Support** (enable `<Nullable>enable</Nullable>`)
- [x] **net9.0 DI Condition Fix** (2026-01-11)

---

## Quality Gates

- Follow SemVer; provide migration guides for major API changes
- Maintain code coverage targets (>= 90%) - Currently at 92.9%
- Continuously verify multi-target compatibility (netstandard2.0/2.1, net8/9/10)

---

## Contribution Guide

Before starting work:
1. Create an issue to share your intent
2. Write related tests
3. Verify existing tests pass (`dotnet test`)
4. Maintain code coverage at 90% or higher

PR Checklist:
- [ ] Add/modify tests
- [ ] Add XML doc comments (public API)
- [ ] Update RELEASE.md
- [ ] No breaking changes (or documented)
