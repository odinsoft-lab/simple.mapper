# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Important**: 
1. Always read and understand the documentation files in the `docs/` folder to maintain comprehensive knowledge of the project. These documents contain critical information about the project's architecture, implementation details, and version history.
2. After completing each task or implementation, immediately update the relevant documentation to reflect the changes. Keep all documentation synchronized with the current state of the codebase.

## Project Overview

SimpleMapper is a high-performance object mapping library for .NET that uses compiled expression trees for efficient runtime mapping. It provides two main mapping approaches:
1. **SimpleMapper**: Direct reflection-based mapping without configuration
2. **SimpleMappingEngine**: Pre-compiled mapping with configuration API for optimal performance

## Build Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Simple.Mapper/Simple.Mapper.csproj

# Build in Release mode
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

## Test Commands

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test tests/Mapper.Tests/Mapper.Tests.csproj

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test
dotnet test --filter "FullyQualifiedName~SimpleMappingEngineTests.Map_SimpleEntity_ShouldMapAllProperties"
```

## Architecture & Design

### Core Components Structure

The library is organized into distinct architectural layers:

1. **Public API Layer** (`src/Simple.Mapper/`)
   - `SimpleMapper.cs`: Static utility class for reflection-based mapping
   - `SimpleMappingEngine.cs`: Main engine class with compiled expression support

2. **Interfaces** (`src/Simple.Mapper/Interfaces/`)
   - `IMappingExpression`: Base mapping configuration interface
   - `IMappingExpression<TSource, TDestination>`: Generic configuration API
   - `IMemberConfigurationExpression`: Member-level mapping configuration

3. **Internal Implementation** (`src/Simple.Mapper/Internal/`)
   - `MappingExpression<TSource, TDestination>`: Implements configuration storage
   - `MemberConfigurationExpression`: Handles member-specific configuration
   - `TypePair`: Struct for type-pair caching keys

4. **Extensions** (`src/Simple.Mapper/Extensions/`)
   - `SimpleMapperExtensions`: Extension methods for fluent mapping

### Mapping Engine Architecture

The `SimpleMappingEngine` uses a sophisticated caching and compilation strategy:

1. **Type Pair Caching**: Uses `ConcurrentDictionary<TypePair, Delegate>` for thread-safe caching of compiled mappers
2. **Expression Tree Compilation**: Builds expression trees on first use, then caches the compiled delegate
3. **Configuration Storage**: Maintains mapping configurations separately from compiled functions
4. **Lock Strategy**: Uses object-level locking during compilation to prevent duplicate compilation

### Key Design Patterns

- **Expression Trees**: Core mapping logic is built using `System.Linq.Expressions` for optimal runtime performance
- **Lazy Compilation**: Mappers are compiled on first use, not during configuration
- **Thread-Safe Caching**: All caching uses `ConcurrentDictionary` for safe multi-threaded access
- **Fluent Configuration**: CreateMap API uses method chaining for intuitive configuration

### Performance Considerations

When modifying the mapping engine:
1. **Compilation happens once per type pair** - stored in `_compiledMappings`
2. **GetOrCompileMapper uses double-check locking** pattern for thread safety
3. **Expression trees are built considering**:
   - Simple types (primitives, strings, DateTime, etc.)
   - Complex nested objects (recursive mapping)
   - Collections (List<T>, arrays, IEnumerable<T>)
   - Nullable type handling

### Testing Structure

Tests are organized by mapping scenario:
- `MapAtRuntime/`: Test entities for runtime mapping scenarios
- `MapAtRuntimeWithCollections/`: Test entities with collection properties
- Each entity pair (Entity1/EntityDTO1, etc.) tests different mapping complexities

## Development Workflow

### Adding New Mapping Features

1. Define interface changes in `Interfaces/` directory
2. Implement internal logic in `Internal/` directory
3. Update `SimpleMappingEngine.CompileMapper` method to handle new configuration
4. Add corresponding tests in `tests/Mapper.Tests/`

### Modifying Expression Compilation

The `CompileMapper<TSource, TDestination>` method in `SimpleMappingEngine.cs` is the heart of the system. When modifying:
1. Understand the expression tree building process (lines 85-199)
2. Consider type compatibility checks (IsSimpleType, IsComplexType, IsCollectionType)
3. Maintain thread safety with proper locking
4. Ensure null checks are properly expressed in the tree

### Type Handling Hierarchy

The system recognizes three type categories:
1. **Simple Types**: Primitives, enums, string, DateTime, Guid, and their nullable versions
2. **Complex Types**: Classes that aren't simple or collections (mapped recursively)
3. **Collection Types**: Arrays, List<T>, IList<T>, IEnumerable<T>, ICollection<T>

## Target Frameworks

The library multi-targets:
- `netstandard2.0`: Maximum compatibility with .NET Framework 4.6.1+
- `net8.0`: Current LTS version
- `net9.0`: Latest .NET version

Tests target `net9.0` and use xUnit as the testing framework.

## Common Pitfalls

1. **Circular References**: The current implementation doesn't handle circular references - will cause stack overflow
2. **Interface Mapping**: Destination types must have parameterless constructors (`where TDestination : new()`)
3. **Collection Element Types**: Only handles generic collections with single type parameter
4. **Custom Mapping**: ForMember implementation is partially complete - custom expressions aren't fully compiled into the expression tree