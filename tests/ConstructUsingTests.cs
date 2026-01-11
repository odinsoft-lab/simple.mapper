using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests;

[Collection("Mapper Tests")]
public class ConstructUsingTests
{
    #region Test Models

    private class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // Destination with parameterless constructor
    private class DestinationWithDefaultConstructor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    // Destination that requires initialization
    private class DestinationWithInitialization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public DestinationWithInitialization()
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = "System";
        }
    }

    // Destination that uses source values in constructor
    private class DestinationWithSourceBasedConstruction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ComputedField { get; set; }

        public DestinationWithSourceBasedConstruction()
        {
        }

        public DestinationWithSourceBasedConstruction(string prefix)
        {
            ComputedField = prefix;
        }
    }

    // Immutable-like destination (set properties in constructor)
    private class ImmutableDestination
    {
        public int Id { get; }
        public string Name { get; }
        public string FullDescription { get; }

        public ImmutableDestination()
        {
        }

        public ImmutableDestination(int id, string name, string description)
        {
            Id = id;
            Name = name;
            FullDescription = $"{name}: {description}";
        }
    }

    #endregion

    #region Basic ConstructUsing Tests

    [Fact]
    public void ConstructUsing_WithCustomConstructor_ShouldUseIt()
    {
        // Arrange
        var engine = Mapper.Reset();
        var constructorCalled = false;

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src =>
            {
                constructorCalled = true;
                return new DestinationWithDefaultConstructor();
            });

        var source = new Source { Id = 1, Name = "Test" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.True(constructorCalled);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);  // Properties should still be mapped
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void ConstructUsing_CanAccessSourceValues()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, DestinationWithSourceBasedConstruction>()
            .ConstructUsing(src =>
            {
                return new DestinationWithSourceBasedConstruction($"Prefix-{src.Id}");
            });

        var source = new Source { Id = 42, Name = "Test" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithSourceBasedConstruction>(source);

        // Assert
        Assert.Equal("Prefix-42", result.ComputedField);
        Assert.Equal(42, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public void ConstructUsing_WithInitialization_ShouldPreserveInitialValues()
    {
        // Arrange
        var engine = Mapper.Reset();
        var testDate = new DateTime(2024, 1, 1);

        engine.CreateMap<Source, DestinationWithInitialization>()
            .ConstructUsing(src =>
            {
                return new DestinationWithInitialization
                {
                    CreatedAt = testDate,
                    CreatedBy = "CustomUser"
                };
            });

        var source = new Source { Id = 1, Name = "Test" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithInitialization>(source);

        // Assert
        Assert.Equal(testDate, result.CreatedAt);  // Should preserve constructor value
        Assert.Equal("CustomUser", result.CreatedBy);
        Assert.Equal(1, result.Id);  // Properties should be mapped on top
        Assert.Equal("Test", result.Name);
    }

    #endregion

    #region Immutable Object Tests

    [Fact]
    public void ConstructUsing_ForImmutableObject_ShouldWork()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, ImmutableDestination>()
            .ConstructUsing(src => new ImmutableDestination(src.Id, src.Name, src.Description));

        var source = new Source { Id = 1, Name = "Test", Description = "A test item" };

        // Act
        var result = engine.MapInstance<Source, ImmutableDestination>(source);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("Test: A test item", result.FullDescription);
    }

    #endregion

    #region Combined with Other Features Tests

    [Fact]
    public void ConstructUsing_WithBeforeMap_ShouldWork()
    {
        // Arrange
        var engine = Mapper.Reset();
        var executionOrder = new List<string>();

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src =>
            {
                executionOrder.Add("ConstructUsing");
                return new DestinationWithDefaultConstructor();
            })
            .BeforeMap((src, dest) =>
            {
                executionOrder.Add("BeforeMap");
            });

        var source = new Source { Id = 1 };

        // Act
        engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.Equal(2, executionOrder.Count);
        Assert.Equal("ConstructUsing", executionOrder[0]);  // Constructor first
        Assert.Equal("BeforeMap", executionOrder[1]);        // Then BeforeMap
    }

    [Fact]
    public void ConstructUsing_WithAfterMap_ShouldWork()
    {
        // Arrange
        var engine = Mapper.Reset();
        var executionOrder = new List<string>();

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src =>
            {
                executionOrder.Add("ConstructUsing");
                return new DestinationWithDefaultConstructor();
            })
            .AfterMap((src, dest) =>
            {
                executionOrder.Add("AfterMap");
            });

        var source = new Source { Id = 1 };

        // Act
        engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.Equal(2, executionOrder.Count);
        Assert.Equal("ConstructUsing", executionOrder[0]);
        Assert.Equal("AfterMap", executionOrder[1]);
    }

    [Fact]
    public void ConstructUsing_WithForMember_ShouldApplyCustomMappings()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src => new DestinationWithDefaultConstructor())
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.ToUpper()));

        var source = new Source { Id = 1, Name = "test" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.Equal("TEST", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void ConstructUsing_WithIgnore_ShouldRespectIgnore()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src => new DestinationWithDefaultConstructor { Description = "Initial" })
            .Ignore(d => d.Description);

        var source = new Source { Id = 1, Name = "Test", Description = "Source Description" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.Equal("Initial", result.Description);  // Should keep constructor value, not map
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    #endregion

    #region Collection Mapping Tests

    [Fact]
    public void ConstructUsing_WithCollection_ShouldWorkForEachItem()
    {
        // Arrange
        var engine = Mapper.Reset();
        var constructCount = 0;

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src =>
            {
                constructCount++;
                return new DestinationWithDefaultConstructor();
            });

        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "First" },
            new Source { Id = 2, Name = "Second" },
            new Source { Id = 3, Name = "Third" }
        };

        // Act
        var results = engine.MapCollection<Source, DestinationWithDefaultConstructor>(sources);

        // Assert
        Assert.Equal(3, constructCount);
        Assert.Equal(3, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal(2, results[1].Id);
        Assert.Equal(3, results[2].Id);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ConstructUsing_WithNullSource_ShouldReturnNull()
    {
        // Arrange
        var engine = Mapper.Reset();
        var constructorCalled = false;

        engine.CreateMap<Source, DestinationWithDefaultConstructor>()
            .ConstructUsing(src =>
            {
                constructorCalled = true;
                return new DestinationWithDefaultConstructor();
            });

        Source source = null;

        // Act
        var result = engine.MapInstance<Source, DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.Null(result);
        Assert.False(constructorCalled);  // Constructor should not be called for null source
    }

    [Fact]
    public void WithoutConstructUsing_ShouldUseDefaultConstructor()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, DestinationWithInitialization>();

        var source = new Source { Id = 1, Name = "Test" };

        // Act
        var result = engine.MapInstance<Source, DestinationWithInitialization>(source);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("System", result.CreatedBy);  // Default constructor value
    }

    #endregion
}
