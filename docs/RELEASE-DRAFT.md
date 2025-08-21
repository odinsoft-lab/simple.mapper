# Release Draft

Date: 2025-08-21

## Highlights
- 🔄 Circular reference handling: AutoMapper-style PreserveReferences and MaxDepth
- ↔️ ReverseMap support: Automatically generate bidirectional mapping configuration
- 🏗️ Architecture improvements: Unified Mapper and MappingEngine; MappingContext-based tracking
- 📚 Docs update: README/DEPLOYMENT/CLAUDE and new ROADMAP added

## Breaking Changes
- Consolidated Mapper and MappingEngine classes
  - Mapper now acts as a static facade over MappingEngine
  - All mapping logic moved under the MappingEngine.Core folder

## New Features
- **Circular reference handling**
  - PreserveReferences(): track object instances to avoid cycles
  - MaxDepth(int): limit recursion depth
  - MappingContext: adopts AutoMapper’s ResolutionContext pattern

- **ReverseMap support**
  - CreateMap<A, B>().ReverseMap() auto-creates reverse mapping
  - Automatic validation of type constraints (new() constraint)

## Improvements
- Expression-based mapping: added cycle detection logic
- Compiler performance: state tracking via MappingContext
- Documentation: clarified project goals and plans in ROADMAP.md

## Migration Guide

### Mapping entities with circular references
```csharp
var engine = new MappingEngine();
engine.CreateMap<Entity10, EntityDTO10>()
  .PreserveReferences()  // track circular references
  .MaxDepth(5);         // limit maximum depth

// Map safely without stack overflow even with cycles
var dto = engine.MapInstance<Entity10, EntityDTO10>(entity);
```

### Using ReverseMap
```csharp
engine.CreateMap<Source, Destination>()
  .ReverseMap();  // automatically create Destination → Source mapping
```

## Known Issues
- Performance tests vary by environment. Official benchmarks will be provided in the next release (BenchmarkDotNet).

## Next
- Add BenchmarkDotNet benchmark project and establish baselines
- Complete ForMember (MapFrom/Condition, etc.) implementation
- Profile system and DI integration
