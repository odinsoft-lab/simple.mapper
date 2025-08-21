# Tasks (Short-term / Immediate)

Updated: 2025-08-21

## Must-do (this sprint)
- [ ] Write release notes: Announce MappingEngine.Map removal and the switch to MapItem; verify README reflects this
- [ ] Split performance tests: Create BenchmarkDotNet project (tests/Benchmarks) and migrate/simplify existing xUnit perf tests
- [ ] Strengthen docs: Add ForMember limitations and ROADMAP link; re-verify DEPLOYMENT is up to date
- [ ] CI setup: Draft GitHub Actions workflow (build + test on push/PR)

## Quick wins (immediately applicable)
- [ ] Improve shared XML docs for MappingEngine/Mapper and include them in the NuGet package
- [ ] Recheck sample code is up to date (uses MapItem, builds on net9)
- [ ] Add comments about performance test tolerances/flakiness (document reasons for keeping tests and alternatives)

## Backlog (short-term)
- [ ] DI integration draft: Design IServiceCollection extension methods (register engine as singleton)
- [ ] Consider option for MapCollection to return an empty list instead of null (option flag)
- [ ] Add tests for additional collection types (array/HashSet/Dictionary) — create skeleton
