# Simple.AutoMapper — Usage Guide

This guide shows how Simple.AutoMapper eliminates repetitive mapping code. Each section shows the manual approach ("Without") and the equivalent mapper call ("With").

---

## Table of Contents

- [Object Mapping](#1-object-mapping)
- [Collection Mapping](#2-collection-mapping)
- [In-Place Update (PUT)](#3-in-place-update-put)
- [Partial Update (PATCH)](#4-partial-update-patch)
- [Property Exclusion](#5-property-exclusion)
- [Custom Property Mapping](#6-custom-property-mapping)
- [Conditional Mapping](#7-conditional-mapping)
- [Null Default Values](#8-null-default-values)
- [Pre/Post Callbacks](#9-prepost-callbacks)
- [Custom Construction](#10-custom-construction)
- [Bidirectional Mapping](#11-bidirectional-mapping)
- [Dependency Injection](#12-dependency-injection)
- [Web API Integration](#13-web-api-integration)
- [EF Core Integration](#14-ef-core-integration)
- [API Reference](#api-reference)

---

## 1. Object Mapping

### Without Simple.AutoMapper

```csharp
var dto = new UserDto();
dto.Id = user.Id;
dto.FirstName = user.FirstName;
dto.LastName = user.LastName;
dto.Email = user.Email;
dto.BirthDate = user.BirthDate;
dto.Address = new AddressDto();
dto.Address.Street = user.Address.Street;
dto.Address.City = user.Address.City;
dto.Address.ZipCode = user.Address.ZipCode;
dto.Orders = user.Orders.Select(o => new OrderDto
{
    OrderId = o.OrderId,
    OrderDate = o.OrderDate,
    TotalAmount = o.TotalAmount
}).ToList();
```

### With Simple.AutoMapper

```csharp
var dto = Mapper.Map<User, UserDto>(user);
```

One line. Nested objects and collections are mapped recursively.

---

## 2. Collection Mapping

### Without Simple.AutoMapper

```csharp
var dtos = new List<UserDto>();
foreach (var user in users)
{
    var dto = new UserDto();
    dto.Id = user.Id;
    dto.FirstName = user.FirstName;
    dto.LastName = user.LastName;
    dto.Email = user.Email;
    // ... repeat for every property
    dtos.Add(dto);
}
```

### With Simple.AutoMapper

```csharp
var dtos = Mapper.Map<User, UserDto>(users);
```

---

## 3. In-Place Update (PUT)

### Without Simple.AutoMapper

```csharp
entity.Name = dto.Name;
entity.Description = dto.Description;
entity.Price = dto.Price;
entity.Stock = dto.Stock;
entity.IsActive = dto.IsActive;
entity.UpdatedAt = DateTime.UtcNow;
// Must add a line here every time a new property is added to the entity
```

### With Simple.AutoMapper

```csharp
Mapper.Map(dto, entity);
entity.UpdatedAt = DateTime.UtcNow;
```

New properties are automatically included — no code changes needed when the model grows.

---

## 4. Partial Update (PATCH)

This is the most impactful use case. HTTP PATCH requires updating only the fields that were provided (non-null).

### Without Simple.AutoMapper

```csharp
if (dto.Name != null) entity.Name = dto.Name;
if (dto.Description != null) entity.Description = dto.Description;
if (dto.Price.HasValue) entity.Price = dto.Price.Value;
if (dto.Stock.HasValue) entity.Stock = dto.Stock.Value;
if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
entity.UpdatedAt = DateTime.UtcNow;
// Every new nullable property needs another if-statement
```

### With Simple.AutoMapper

```csharp
Mapper.Patch(dto, entity);
entity.UpdatedAt = DateTime.UtcNow;
```

`Patch` skips null source properties automatically. No if-chains needed.

### Map vs Patch difference

```csharp
var dto = new UpdateDto { Name = "New Name", Price = null };
var entity = new Product { Name = "Old", Price = 50m, Stock = 200 };

Mapper.Map(dto, entity);
// Name="New Name", Price=0 (null overwritten to default), Stock=0

Mapper.Patch(dto, entity);
// Name="New Name", Price=50 (preserved), Stock=200 (preserved)
```

---

## 5. Property Exclusion

### Without Simple.AutoMapper

```csharp
var dto = new UserDto();
dto.Id = user.Id;
dto.FirstName = user.FirstName;
dto.LastName = user.LastName;
dto.Email = user.Email;
// dto.Password = user.Password;        // manually skip
// dto.InternalNotes = user.InternalNotes;  // manually skip
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<User, UserDto>()
    .Ignore(d => d.Password)
    .Ignore(d => d.InternalNotes);

var dto = Mapper.Map<User, UserDto>(user);
```

---

## 6. Custom Property Mapping

When property names differ or values need transformation.

### Without Simple.AutoMapper

```csharp
var dto = new EmployeeDto();
dto.Id = emp.Id;
dto.FullName = $"{emp.FirstName} {emp.LastName}";
dto.Age = DateTime.Now.Year - emp.BirthYear;
dto.Department = emp.Department;
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<Employee, EmployeeDto>()
    .ForMember<string>(d => d.FullName,
        opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
    .ForMember<int>(d => d.Age,
        opt => opt.MapFrom(s => DateTime.Now.Year - s.BirthYear));

var dto = Mapper.Map<Employee, EmployeeDto>(emp);
```

`Department` is auto-mapped by matching name. Only mismatched or computed properties need `ForMember`.

---

## 7. Conditional Mapping

### Without Simple.AutoMapper

```csharp
var dto = new UserDto();
dto.Id = user.Id;
dto.Name = user.Name;
if (user.IsEmailVerified)
{
    dto.Email = user.Email;
}
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember<string>(d => d.Email, opt => {
        opt.MapFrom(s => s.Email);
        opt.Condition(s => s.IsEmailVerified);
    });

var dto = Mapper.Map<User, UserDto>(user);
```

---

## 8. Null Default Values

### Without Simple.AutoMapper

```csharp
var dto = new UserDto();
dto.Id = user.Id;
dto.DisplayName = user.Nickname ?? "Anonymous";
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<User, UserDto>()
    .ForMember<string>(d => d.DisplayName, opt => {
        opt.MapFrom(s => s.Nickname);
        opt.NullSubstitute("Anonymous");
    });

var dto = Mapper.Map<User, UserDto>(user);
```

---

## 9. Pre/Post Callbacks

### Without Simple.AutoMapper

```csharp
var dto = new UserDto();
dto.MappedAt = DateTime.UtcNow;         // before
dto.Id = user.Id;
dto.FirstName = user.FirstName;
dto.LastName = user.LastName;
// ... all properties ...
dto.FullName = $"{dto.FirstName} {dto.LastName}";  // after
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<User, UserDto>()
    .BeforeMap((src, dest) => dest.MappedAt = DateTime.UtcNow)
    .AfterMap((src, dest) => dest.FullName = $"{dest.FirstName} {dest.LastName}");

var dto = Mapper.Map<User, UserDto>(user);
```

Execution order: BeforeMap → property mapping → AfterMap.

---

## 10. Custom Construction

### Without Simple.AutoMapper

```csharp
var record = new ImmutableRecord(source.Id, source.Name, source.Email);
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<Source, ImmutableRecord>()
    .ConstructUsing(src => new ImmutableRecord(src.Id, src.Name, src.Email));

var record = Mapper.Map<Source, ImmutableRecord>(source);
```

Useful when the destination type requires constructor parameters, or when you need factory logic centralized in one place instead of scattered across the codebase.

---

## 11. Bidirectional Mapping

### Without Simple.AutoMapper

```csharp
// Entity → DTO (manual)
var dto = new UserDto { Id = entity.Id, Name = entity.Name, Email = entity.Email };

// DTO → Entity (manual, duplicated logic)
var entity = new User { Id = dto.Id, Name = dto.Name, Email = dto.Email };
```

### With Simple.AutoMapper

```csharp
Mapper.CreateMap<User, UserDto>()
    .ReverseMap();

var dto = Mapper.Map<User, UserDto>(entity);      // forward
var entity = Mapper.Map<UserDto, User>(dto);       // reverse (auto-generated)
```

---

## 12. Dependency Injection

### Without Simple.AutoMapper

```csharp
// Manual mapping scattered across controllers, services, repositories...
public class UserService
{
    public UserDto GetUser(int id)
    {
        var entity = _db.Users.Find(id);
        return new UserDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            // ... repeat for every property
        };
    }
}
```

### With Simple.AutoMapper

```csharp
// Program.cs — register once
builder.Services.AddSimpleMapper(cfg => {
    cfg.AddProfile<UserProfile>();
});

// Service — inject and use
public class UserService
{
    private readonly ISimpleMapper _mapper;

    public UserService(ISimpleMapper mapper) => _mapper = mapper;

    public UserDto GetUser(int id)
    {
        var entity = _db.Users.Find(id);
        return _mapper.Map<User, UserDto>(entity);
    }
}
```

### Profile — grouping mappings

```csharp
public class UserProfile : Profile
{
    protected override void Configure()
    {
        CreateMap<User, UserDto>()
            .ForMember<string>(d => d.FullName,
                opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ReverseMap();

        CreateMap<Address, AddressDto>();
    }
}
```

---

## 13. Web API Integration

### Without Simple.AutoMapper

```csharp
[HttpGet("{id}")]
public ActionResult<ProductDto> Get(int id)
{
    var entity = _db.Products.Find(id);
    return new ProductDto
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Price = entity.Price,
        Stock = entity.Stock,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };
}

[HttpPatch("{id}")]
public ActionResult Patch(int id, UpdateProductDto dto)
{
    var entity = _db.Products.Find(id);
    if (dto.Name != null) entity.Name = dto.Name;
    if (dto.Description != null) entity.Description = dto.Description;
    if (dto.Price.HasValue) entity.Price = dto.Price.Value;
    if (dto.Stock.HasValue) entity.Stock = dto.Stock.Value;
    if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
    entity.UpdatedAt = DateTime.UtcNow;
    _db.SaveChanges();
    return Ok();
}
```

### With Simple.AutoMapper

```csharp
[HttpGet("{id}")]
public ActionResult<ProductDto> Get(int id)
{
    var entity = _db.Products.Find(id);
    return _mapper.Map<Product, ProductDto>(entity);
}

[HttpPatch("{id}")]
public ActionResult Patch(int id, UpdateProductDto dto)
{
    var entity = _db.Products.Find(id);
    _mapper.Patch(dto, entity);
    entity.UpdatedAt = DateTime.UtcNow;
    _db.SaveChanges();
    return Ok();
}
```

---

## 14. EF Core Integration

### Without Simple.AutoMapper

```csharp
// Read
var users = await db.Users.AsNoTracking().Include(u => u.Address).ToListAsync();
var dtos = new List<UserDto>();
foreach (var u in users)
{
    dtos.Add(new UserDto
    {
        Id = u.Id, FirstName = u.FirstName, LastName = u.LastName,
        Address = new AddressDto { Street = u.Address.Street, City = u.Address.City }
    });
}

// Create
var entity = new User { Name = dto.Name, Email = dto.Email, /* ... */ };
await db.Users.AddAsync(entity);

// Update
var entity = await db.Users.FindAsync(id);
entity.Name = dto.Name;
entity.Email = dto.Email;
entity.Price = dto.Price;
// ... repeat for all properties
await db.SaveChangesAsync();
```

### With Simple.AutoMapper

```csharp
// Read
var users = await db.Users.AsNoTracking().Include(u => u.Address).ToListAsync();
var dtos = Mapper.Map<User, UserDto>(users);

// Create
var entity = Mapper.Map<CreateUserDto, User>(dto);
await db.Users.AddAsync(entity);

// Full Update (PUT)
var entity = await db.Users.FindAsync(id);
Mapper.Map(dto, entity);
await db.SaveChangesAsync();

// Partial Update (PATCH)
var entity = await db.Users.FindAsync(id);
Mapper.Patch(dto, entity);
await db.SaveChangesAsync();
```

---

## API Reference

### Map — 4 overloads (full property copy)

| Overload | Description |
|----------|-------------|
| `Map<TS, TD>(source)` | Create new TD from TS |
| `Map<TD>(object source)` | Create new TD, source type inferred |
| `Map<TS, TD>(IEnumerable<TS>)` | Map collection to List&lt;TD&gt; |
| `Map<TS, TD>(source, dest)` | In-place update (overwrites all) |

### Patch — 4 overloads (null-skip semantics)

| Overload | Description |
|----------|-------------|
| `Patch<TS, TD>(source)` | Create new TD, skip null properties |
| `Patch<TD>(object source)` | Create new TD, source type inferred, skip null |
| `Patch<TS, TD>(IEnumerable<TS>)` | Patch collection to List&lt;TD&gt; |
| `Patch<TS, TD>(source, dest)` | In-place partial update (skip null) |

### Configuration

| Method | Description |
|--------|-------------|
| `CreateMap<TS, TD>()` | Register a mapping pair |
| `.Ignore(d => d.Prop)` | Exclude a property |
| `.ForMember<T>(d => d.Prop, opt => opt.MapFrom(...))` | Custom mapping |
| `opt.Condition(s => bool)` | Conditional mapping |
| `opt.NullSubstitute(value)` | Default when source is null |
| `.BeforeMap((s, d) => ...)` | Pre-mapping callback |
| `.AfterMap((s, d) => ...)` | Post-mapping callback |
| `.ConstructUsing(s => new T(...))` | Custom construction |
| `.ReverseMap()` | Bidirectional mapping |
| `.PreserveReferences()` | Circular reference tracking |
| `.MaxDepth(n)` | Recursion depth limit |

### Dependency Injection

```csharp
// Program.cs
builder.Services.AddSimpleMapper(cfg => {
    cfg.AddProfile<MyProfile>();
});

// Or auto-scan assembly
builder.Services.AddSimpleMapper(typeof(MyProfile).Assembly);
```

### Supported Frameworks

netstandard2.0, netstandard2.1, net8.0, net9.0, net10.0

DI support: netstandard2.1+ only (netstandard2.0 excluded)

---

## Samples

| Sample | Location | Demonstrates |
|--------|----------|--------------|
| Console | `samples/Console/` | All 19 features with static `Mapper` |
| WebAPI | `samples/WebAPI/` | REST API with DI, PUT/PATCH endpoints |
