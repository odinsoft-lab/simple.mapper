# Simple.AutoMapper Roadmap

This document outlines planned improvements for the medium to long term. Priorities and scope may change based on project needs.

Last updated: 2026-01-11

## Goal Axes
- Feature completeness: expand configuration (ForMember, etc.) and mapping scenario coverage
- Performance/stability: predictable performance and low GC pressure under both low and high loads
- Usability/DX: clear APIs, documentation, diagnostics/validation tools, IDE friendliness
- Integration: .NET DI, EF Core, CI/CD, NuGet quality metadata

## Completed Features (v1.0.9)

- Configuration API
  - ForMember implementation: MapFrom (expression) ✅
  - Condition (conditional mapping) ✅
  - NullSubstitute (null value replacement) ✅
  - ConstructUsing (custom object construction) ✅
  - BeforeMap/AfterMap hooks ✅
  - ReverseMap support ✅
  - Ignore property ✅
  - PreserveReferences (circular reference handling) ✅
  - MaxDepth (recursion limit) ✅
- DI Integration
  - Microsoft.Extensions.DependencyInjection integration ✅
  - Profile scanning ✅
  - AddSimpleMapper() extension ✅
- Target Framework
  - net10.0 support ✅

## Feature Roadmap (mid-term, 1–3 months)

- Configuration API enhancements
  - ConvertUsing (full type conversion control)
  - TypeConverter (global type converters, e.g., string → DateTime)
  - Rule-based Ignore/Include options
- Collections/type coverage expansion
  - Mapping for HashSet/Dictionary
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

- Clean up XML doc comments and include in NuGet; SourceLink/symbol packages
- GitHub Actions: automate build/test/pack/release draft

## Long-term Roadmap (3–6 months+)

- IQueryable projections (expression trees translatable by EF Core)
- C# Source Generator for static mappers (with runtime fallback)
- Rule-based naming/flattening (e.g., `Parent.ChildId` ↔ `ParentChildId`)
- Analyzer package: detect unmapped members and risky conversions
- Web documentation site (DocFX/Docusaurus) and sample gallery expansion

## Quality Gates/Policies

- Follow SemVer; provide migration guides for major API changes
- Maintain code coverage targets (≥ 85%) ✅ Currently at 87%
- Continuously verify multi-target compatibility (netstandard2.0/2.1, net8/9/10)

## Breaking Change Management

- No API renames in 1.0.x. For major future features, consider experimental flags/pre-release channels.
