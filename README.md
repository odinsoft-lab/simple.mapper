# Simple.AutoMapper

High-performance mapping engine built around a CreateMap configuration API.

## Key Features

1) Compiled expressions

- Compiles mapping logic using expression trees
- Compiles on the first run, then reuses the cached delegate
- Eliminates reflection overhead at runtime

2) Per-type caching

- Thread-safe caching with ConcurrentDictionary
- Stores compiled mapping functions per TypePair
- Immediately reused for subsequent mappings of the same types

3) CreateMap configuration API

```csharp
engine.CreateMap<Entity1, EntityDTO1>()
  .Ignore(d => d.SomeProperty) // Ignore specific property
  .ForMember(d => d.CustomProp, opt => opt.MapFrom(s => s.SourceProp));
```

4) Performance optimizations

- First run: includes compilation
- Subsequent runs: directly call the cached compiled function
- Parallel execution: thread-safe concurrent usage

## How to Use

```csharp
// 1) Create the engine and configure mappings (once at app startup)
var engine = new MappingEngine();
engine.CreateMap<UserEntity, UserDTO>();
engine.CreateMap<AddressEntity, AddressDTO>();

// 2) Execute mappings (reused many times)
var userDto = engine.Map<UserEntity, UserDTO>(userEntity);
var userDtos = engine.MapList<UserEntity, UserDTO>(userEntities);
```

## Performance Characteristics

1) Compile caching: significantly faster after the first run (warm-up)

2) Memory efficiency: only one compiled function per type pair is kept

3) Concurrency: safe in multithreaded environments

4) Predictable performance: consistent throughput after caching

## Advantages Over the basic Simple.AutoMapper

| Aspect   | Basic Simple.AutoMapper       | MappingEngine                 |
|----------|--------------------------|-------------------------------------|
| Performance | Reflection on every call | Compile once, then use cache         |
| Memory    | Low                       | Per-type cache (balanced)            |
| Configuration | None                    | Pre-configure with CreateMap         |
| Extensibility | Limited                  | Supports Ignore, ForMember, and more |

Tests show a notable speedup when mapping 1,000 entities after the compiled mapping is cached.
  