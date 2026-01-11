using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests;

[Collection("Mapper Tests")]
public class BeforeAfterMapTests
{
    #region Test Models

    private class Source
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    private class Destination
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? MappedAt { get; set; }
        public bool WasProcessed { get; set; }
    }

    #endregion

    #region BeforeMap Tests

    [Fact]
    public void BeforeMap_ShouldExecuteBeforePropertyMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        var beforeMapExecuted = false;
        Destination capturedDestination = null;

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                beforeMapExecuted = true;
                capturedDestination = dest;
                // At this point, dest should have default values
                dest.WasProcessed = true;
            });

        var source = new Source { FirstName = "John", LastName = "Doe", Age = 30 };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.True(beforeMapExecuted);
        Assert.NotNull(capturedDestination);
        Assert.Same(result, capturedDestination);
        Assert.True(result.WasProcessed);
        Assert.Equal("John", result.FirstName); // Property mapping should still occur after BeforeMap
    }

    [Fact]
    public void BeforeMap_ShouldHaveAccessToSourceValues()
    {
        // Arrange
        var engine = Mapper.Reset();
        string capturedFirstName = null;

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                capturedFirstName = src.FirstName;
            });

        var source = new Source { FirstName = "Jane" };

        // Act
        engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("Jane", capturedFirstName);
    }

    [Fact]
    public void BeforeMap_CanModifyDestinationBeforeMapping()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                // Set a computed value before mapping
                dest.FullName = $"{src.FirstName} {src.LastName}";
            });

        var source = new Source { FirstName = "John", LastName = "Doe" };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    #endregion

    #region AfterMap Tests

    [Fact]
    public void AfterMap_ShouldExecuteAfterPropertyMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        var afterMapExecuted = false;
        string capturedFirstName = null;

        engine.CreateMap<Source, Destination>()
            .AfterMap((src, dest) =>
            {
                afterMapExecuted = true;
                // At this point, dest should have mapped values
                capturedFirstName = dest.FirstName;
            });

        var source = new Source { FirstName = "John" };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.True(afterMapExecuted);
        Assert.Equal("John", capturedFirstName);
    }

    [Fact]
    public void AfterMap_CanModifyDestinationAfterMapping()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, Destination>()
            .AfterMap((src, dest) =>
            {
                // Compute FullName after FirstName and LastName are mapped
                dest.FullName = $"{dest.FirstName} {dest.LastName}";
                dest.MappedAt = DateTime.UtcNow;
            });

        var source = new Source { FirstName = "John", LastName = "Doe" };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("John Doe", result.FullName);
        Assert.NotNull(result.MappedAt);
    }

    [Fact]
    public void AfterMap_CanPerformValidation()
    {
        // Arrange
        var engine = Mapper.Reset();
        var validationPassed = false;

        engine.CreateMap<Source, Destination>()
            .AfterMap((src, dest) =>
            {
                // Validation logic
                if (dest.Age >= 0 && dest.Age <= 150)
                {
                    validationPassed = true;
                }
            });

        var source = new Source { Age = 30 };

        // Act
        engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.True(validationPassed);
    }

    #endregion

    #region Combined BeforeMap and AfterMap Tests

    [Fact]
    public void BeforeMap_And_AfterMap_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var engine = Mapper.Reset();
        var executionOrder = new List<string>();

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                executionOrder.Add("BeforeMap");
            })
            .AfterMap((src, dest) =>
            {
                executionOrder.Add("AfterMap");
            });

        var source = new Source { FirstName = "John" };

        // Act
        engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal(2, executionOrder.Count);
        Assert.Equal("BeforeMap", executionOrder[0]);
        Assert.Equal("AfterMap", executionOrder[1]);
    }

    [Fact]
    public void BeforeMap_And_AfterMap_WithPropertyMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        var stateInBeforeMap = "";
        var stateInAfterMap = "";

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                stateInBeforeMap = dest.FirstName ?? "NULL";
            })
            .AfterMap((src, dest) =>
            {
                stateInAfterMap = dest.FirstName ?? "NULL";
            });

        var source = new Source { FirstName = "John" };

        // Act
        engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("NULL", stateInBeforeMap); // Before mapping, FirstName is null
        Assert.Equal("John", stateInAfterMap);  // After mapping, FirstName is "John"
    }

    [Fact]
    public void BeforeMap_And_AfterMap_CanWorkWithForMember()
    {
        // Arrange
        var engine = Mapper.Reset();

        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.FirstName.ToUpper()))
            .BeforeMap((src, dest) =>
            {
                dest.WasProcessed = true;
            })
            .AfterMap((src, dest) =>
            {
                dest.FullName = $"{dest.FirstName} {dest.LastName}";
            });

        var source = new Source { FirstName = "John", LastName = "Doe" };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.True(result.WasProcessed);
        Assert.Equal("JOHN", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("JOHN Doe", result.FullName);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void BeforeMap_WithNullSource_ShouldNotExecute()
    {
        // Arrange
        var engine = Mapper.Reset();
        var beforeMapExecuted = false;

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                beforeMapExecuted = true;
            });

        Source source = null;

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Null(result);
        Assert.False(beforeMapExecuted); // BeforeMap should not execute for null source
    }

    [Fact]
    public void AfterMap_WithNullSource_ShouldNotExecute()
    {
        // Arrange
        var engine = Mapper.Reset();
        var afterMapExecuted = false;

        engine.CreateMap<Source, Destination>()
            .AfterMap((src, dest) =>
            {
                afterMapExecuted = true;
            });

        Source source = null;

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Null(result);
        Assert.False(afterMapExecuted); // AfterMap should not execute for null source
    }

    [Fact]
    public void BeforeMap_And_AfterMap_WithCollectionMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        var beforeMapCount = 0;
        var afterMapCount = 0;

        engine.CreateMap<Source, Destination>()
            .BeforeMap((src, dest) =>
            {
                beforeMapCount++;
            })
            .AfterMap((src, dest) =>
            {
                afterMapCount++;
            });

        var sources = new List<Source>
        {
            new Source { FirstName = "John" },
            new Source { FirstName = "Jane" },
            new Source { FirstName = "Bob" }
        };

        // Act
        var results = engine.MapCollection<Source, Destination>(sources);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(3, beforeMapCount); // BeforeMap should execute for each item
        Assert.Equal(3, afterMapCount);  // AfterMap should execute for each item
    }

    #endregion
}
