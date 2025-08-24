# Contributing to Simple.AutoMapper

Thanks for your interest in contributing! This project intentionally implements only the essential features of AutoMapper to keep the codebase simple and maintainable. Please read this guide before opening a PR.

## Scope and philosophy (MVP)

We focus on the most-used mapping scenarios:

- CreateMap<TSource, TDestination>
- Map one object, map collections (`IEnumerable<T>` → `List<T>`), simple reflection overload
- Ignore(d => d.Member)
- ReverseMap() — basic inversion only
- ForMember(...).MapFrom(...) — property-to-property mapping only

Out of scope for MVP (do not propose unless discussed first):

- Profiles/MapperConfiguration/DI integration/global options
- Full configuration validation, Before/After hooks, Include/IncludeBase
- Custom converters/resolvers, naming conventions, flattening/unflattening
- IQueryable projection/expression mapping, DataReader/Enum extensions
- Advanced circular reference/depth handling beyond a basic guard

## Complexity guardrails

- Keep the public API surface minimal
- No external runtime deps beyond BCL
- Compiled mapping path must avoid runtime reflection beyond the initial compile
- No magic conventions: property names must match exactly
- Each feature must include unit tests and XML docs
- Keep PRs small (aim ≤ ~150 net LOC per module)

## Documentation and code comments

- All Markdown docs must be in English
- All public/protected APIs must have English XML doc comments
- Keep comments concise and actionable

## Coding standards

- C#: Follow .NET design guidelines; use `readonly`, `sealed`, `in`/`ref` where appropriate
- Prefer expressions/linq clarity over micro-optimizations unless measured
- Avoid unnecessary abstractions; favor simple, direct code

## Tests

- Add/adjust unit tests for any behavior change
- Keep tests deterministic; no timing-sensitive asserts when possible
- For performance, use BenchmarkDotNet (tests/Benchmarks) rather than timing asserts in xUnit

## Build and warnings

- The repo builds with TreatWarningsAsErrors; your PR must be warning-clean
- Public XML docs are required for any new exposed APIs

## Commit messages

- Use imperative mood (e.g., "Implement MapFrom for compiled mapping")
- Reference issues when applicable (e.g., "Refs #123")

## Pull request checklist (Definition of Done)

- [ ] Fits MVP scope (or labeled and justified as Backlog)
- [ ] Unit tests added/updated; all tests pass locally
- [ ] Public/protected APIs have XML docs; no new warnings
- [ ] Documentation updated (README/release notes/TASK if behavior changes)
- [ ] Benchmarks updated/added if performance is relevant

We appreciate your contributions and focus on simplicity!