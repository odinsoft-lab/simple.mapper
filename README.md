# Simple.AutoMapper

[![NuGet](https://img.shields.io/nuget/v/Simple.AutoMapper.svg)](https://www.nuget.org/packages/Simple.AutoMapper/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Downloads](https://img.shields.io/nuget/dt/Simple.AutoMapper.svg)](https://www.nuget.org/packages/Simple.AutoMapper/)
[![Coverage](https://img.shields.io/badge/coverage-92.9%25-brightgreen.svg)]()

High-performance object mapping for .NET with expression tree compilation. Simple API, powerful configuration options.

> **Latest** - Patch overloads (new object, type-inferred, collection), Map in-place via `ISimpleMapper`. See [Release Notes](docs/RELEASE.md).

## Installation

```powershell
dotnet add package Simple.AutoMapper
```

**Target Frameworks:** netstandard2.0, netstandard2.1, net8.0, net9.0, net10.0

## Quick Start

```csharp
using Simple.AutoMapper.Core;

// Map — copy all properties to new object
var dto = Mapper.Map<User, UserDto>(user);

// Patch — copy only non-null properties (HTTP PATCH scenario)
Mapper.Patch(partialDto, existingEntity);
```

### Before & After

```csharp
// ❌ Without Simple.AutoMapper — manual property-by-property copy
var dto = new UserDto();
dto.Id = user.Id;
dto.FirstName = user.FirstName;
dto.LastName = user.LastName;
dto.Email = user.Email;
// ... repeat for every property, every model, every service

// ✅ With Simple.AutoMapper — one line
var dto = Mapper.Map<User, UserDto>(user);
```

For the full before/after comparison of every feature, see the **[Usage Guide](docs/GUIDE.md)**.

## Performance

- Expression tree compilation for fast subsequent mappings
- Thread-safe caching of compiled mappers
- First mapping incurs compilation cost; subsequent calls are optimized

## Test Coverage

- 242+ tests passing
- 92.9% line coverage, 88.8% branch coverage

## Documentation

- **[Usage Guide](docs/GUIDE.md)** — Before/after examples, API reference, patterns
- [Release Notes](docs/RELEASE.md)
- [Tasks & Roadmap](docs/TASK.md)
- [Deployment Guide](docs/DEPLOYMENT.md)
- [Contributing](docs/CONTRIBUTING.md)

## Samples

| Sample | Location | Demonstrates |
|--------|----------|--------------|
| Console | [`samples/Console/`](samples/Console/) | All 19 features with static `Mapper` |
| WebAPI | [`samples/WebAPI/`](samples/WebAPI/) | REST API with DI, PUT/PATCH endpoints |

## License

MIT License - see [LICENSE.md](LICENSE.md)

## Team

**Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect
- **YUJIN** - Senior Developer & Exchange Integration Specialist
- **SEJIN** - Software Developer & API Implementation

---

**Built with care by the ODINSOFT Team** | [GitHub](https://github.com/odinsoft-lab/simple.mapper)
