# Tasks (Short-term / Immediate)

Updated: 2025-08-24

## Product scope (MVP = essential only)

Keep the library simple by focusing on the most-used 20% of features:

- CreateMap<TSource, TDestination>
- Map single object: MapInstance<TSource, TDestination>(source)
- Map from object (reflection): Map<TDestination>(object source)
- Map collections: MapCollection<TSource, TDestination>(IEnumerable<TSource>) → List<TDestination>
- Ignore(d => d.Member)
- ReverseMap() — basic inversion only
- ForMember(...).MapFrom(...) — property-to-property mapping only (no complex resolvers)

Non-goals (out of MVP scope):
- Profiles, MapperConfiguration, DI integration, global options
- Config validation (full AssertConfigurationIsValid)
- BeforeMap/AfterMap, ForAllMembers, Include/IncludeBase, inheritance magic
- Custom type converters/value resolvers/ConvertUsing
- Naming conventions, flattening/unflattening helpers
- IQueryable projection/expression mapping, DataReader/Enum extensions
- Advanced circular reference management beyond a basic guard

## Feature comparison vs. AutoMapper (master/src)

Implemented (MVP):
- CreateMap<TSource, TDestination> basic configuration API
- Map single object: MapInstance<TSource, TDestination>(source)
- Map from object (reflection): Map<TDestination>(object source)
- Map collections: MapCollection<TSource, TDestination>(IEnumerable<TSource>) → List<TDestination>
- Expression-compiled mappers with caching per TypePair
- ReverseMap() — basic reverse configuration (no per-member reverse rules)
- Ignore(d => d.Member) — honored in compiled mapping
- Basic circular reference guard and destination instance caching
- Public XML docs added for exposed APIs

Partially implemented:
- ForMember(...).MapFrom(...) — stored in config but not applied in compiled mapping yet
- PreserveReferences() — flag and caching exist; behavior is simplistic
- MaxDepth(...) — tracked superficially; not decremented/incremented in all paths; one test skipped
- Collections — List<T> fully; arrays via reflection mapping; no HashSet/Dictionary/ICollection special handling
- Null handling — simple propagation; no NullSubstitute/Condition

Not implemented (out of scope for MVP, candidates for future):
- Profiles/MapperConfiguration/IMapper abstraction and DI integration
- AssertConfigurationIsValid and config validation
- BeforeMap/AfterMap hooks, ForAllMembers
- Include/IncludeBase and inheritance mapping
- Custom type converters/value resolvers/ConvertUsing
- Naming conventions, flattening/unflattening
- Projection (IQueryable) and expression mapping
- DataReader/Record mapping, Enum mapping extensions
- Constructor parameter mapping and non-default constructors
- Global options (AllowNullCollections, AllowNullDestinationValues, etc.)

Test status highlights:
- 27 passing, 2 skipped (MaxDepth guard and ReverseMap + Ignore semantics pending)

Gap summary and priorities:
P1) Wire up ForMember.MapFrom in compiled mapping (highest user value)
P1) Strengthen PreserveReferences/MaxDepth (fix skipped tests)
P1) Collections minimal expansion (ensure IEnumerable<T>→List<T>, arrays; keep HashSet/Dictionary for later)
P2) Validation API (lightweight AssertConfiguration) and DI helpers (optional)
P2) ReverseMap + Ignore semantics (document policy and tests)

## Must-do (this sprint) — essentials only
- [ ] Implement ForMember.MapFrom in compiled pipeline (property-level mapping only)
- [ ] PreserveReferences/MaxDepth: implement basic enter/exit depth tracking and reuse cached instances; unskip MaxDepth test
- [ ] Collections (minimal): validate IEnumerable<T>→List<T> and arrays; add unit tests for null/empty and simple/complex elements
- [ ] Benchmarks: Create BenchmarkDotNet project (tests/Benchmarks); move perf assertions out of xUnit
- [ ] Docs: Reflect MVP scope/limitations in README and releases
- [ ] CI: Draft GitHub Actions workflow (build + test on push/PR; pack on tag)
- [ ] NuGet: Include XML docs and SourceLink, publish snupkg

## Quick wins (immediately applicable)
- [x] Improve public XML docs (done) and ensure they ship in the NuGet package
- [x] Fix Markdown generic rendering across docs (wrap `List<T>`, `Map<T>`, etc. in backticks)
- [ ] Document performance test tolerances/flakiness and alternatives (BenchmarkDotNet)
- [x] Update sample: add in-place update usage and wire into Program.cs

## Backlog (short-term)
- [ ] DI integration draft: IServiceCollection extensions (singleton engine)
- [ ] Option: Make MapCollection return empty list instead of null (configurable)
- [ ] Add tests for additional collection types (array/HashSet/Dictionary)
- [ ] Basic configuration validation (detect unmapped members when requested)
- [ ] ReverseMap + Ignore semantics: finalize policy and expand test coverage (P2)
- [ ] Advanced circular ref/depth features beyond basic guard (only if strong demand)

## Complexity guardrails (to avoid scope creep)

- Public API surface stays minimal (no profiles/DI/validation in MVP)
- No external runtime dependencies beyond BCL
- Compiled mapping path should not add reflection at runtime beyond initial compile
- No magic conventions beyond exact-name property matching
- Keep feature flags out; prefer explicit, simple behavior
- Each new feature must include: (a) unit tests, (b) XML docs, (c) ≤ ~150 LOC net increase per module

## PR checklist (Definition of Done)

- [ ] Fits MVP scope (or explicit “Backlog” label if not)
- [ ] Tests: added/updated and green locally
- [ ] XML docs for any public/protected API
- [ ] No new warnings; TreatWarningsAsErrors passes
- [ ] Docs updated (README/release notes/TASK if behavior changes)
