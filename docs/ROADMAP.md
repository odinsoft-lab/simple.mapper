# Simple.AutoMapper Roadmap

This document outlines planned improvements for the medium to long term. Priorities and scope may change based on project needs.

Last updated: 2025-08-21

## Goal Axes
- Feature completeness: expand configuration (ForMember, etc.) and mapping scenario coverage
- Performance/stability: predictable performance and low GC pressure under both low and high loads
- Usability/DX: clear APIs, documentation, diagnostics/validation tools, IDE friendliness
- Integration: .NET DI, EF Core, CI/CD, NuGet quality metadata

## Feature Roadmap (mid-term, 1–3 months)
- Configuration API enhancements
  - ForMember implementation: MapFrom (expression), Constant, Condition, NullSubstitute
  - Keep Ignore as-is; add rule-based Ignore/Include options
  - ConstructUsing, BeforeMap/AfterMap hooks
  - ReverseMap support (generate bidirectional configuration) ✅ Done
- Collections/type coverage expansion
  - Mapping for arrays/HashSet/Dictionary
  - Strategies for record, init-only, and read-only collections
  - Polymorphic mapping options (interface/base → derived)
- Validation/diagnostics
  - Configuration validation API (detect unmapped members, type mismatches, impossible conversions)
  - Runtime diagnostics with log levels/event source

## Performance Roadmap (mid-term, 1–3 months)
- BenchmarkDotNet micro-benchmark suite and baseline
- Cache improvements: warm-up, capacity/expiration policy, metrics
- Allocation reduction: streaming option for MapCollection (alternative designs for IReadOnlyList/Span inputs)

## Integration/Platform Roadmap (mid-term, 1–3 months)
- Microsoft.Extensions.DependencyInjection integration (singleton engine, profile scanning)
- Clean up XML doc comments and include in NuGet; SourceLink/symbol packages
- GitHub Actions: automate build/test/pack/release draft

## Long-term Roadmap (3–6 months+)
- IQueryable projections (expression trees translatable by EF Core)
- C# Source Generator for static mappers (with runtime fallback)
- Rule-based naming/flattening (e.g., `Parent.ChildId` ↔ `ParentChildId`)
- Circular reference handling/object identity preservation/max depth ✅ Done (PreserveReferences, MaxDepth)
- Analyzer package: detect unmapped members and risky conversions
- Web documentation site (DocFX/Docusaurus) and sample gallery expansion

## Quality Gates/Policies
- Follow SemVer; provide migration guides for major API changes
- Maintain code coverage targets (e.g., ≥ 85%) and performance budgets
- Continuously verify multi-target compatibility (netstandard2.0/2.1, net8/9)

## Breaking Change Management
- MappingEngine.Map → MapItem transition completed. Further refine release notes/migration guide in upcoming releases.
- For major future features, consider experimental flags/pre-release channels.
