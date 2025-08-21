# Tasks (Short-term / Immediate)

Updated: 2025-08-21

## Must-do (this sprint)
- [ ] Benchmarks: Create BenchmarkDotNet project (tests/Benchmarks) and migrate perf assertions out of xUnit
- [ ] Docs: Ensure ForMember limitations and experimental PreserveReferences/MaxDepth notes are consistent across README and RELEASE-NOTES
- [ ] CI: Draft GitHub Actions workflow (build + test on push/PR; pack on tag)
- [ ] NuGet: Include XML docs and SourceLink, ensure snupkg is published

## Quick wins (immediately applicable)
- [ ] Improve XML docs for MappingEngine/Mapper and include them in the NuGet package
- [x] Update sample code: add in-place update usage and wire into Program.cs
- [x] Fix Markdown generic rendering across docs (wrap `List<T>`, `Map<T>`, etc. in backticks)
- [ ] Document performance test tolerances/flakiness and alternatives (BenchmarkDotNet)

## Backlog (short-term)
- [ ] DI integration draft: Design IServiceCollection extension methods (register engine as singleton)
- [ ] Option: Make MapCollection return empty list instead of null (configurable)
- [ ] Add tests for additional collection types (array/HashSet/Dictionary) — create skeleton
- [x] Add tests for in-place update behavior (copy, null propagation)
