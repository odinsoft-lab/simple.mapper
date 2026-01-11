using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests;

[Collection("Mapper Tests")]
public class ConditionAndNullSubstituteTests
{
    #region Test Models

    private class Source
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public Address Address { get; set; }
    }

    private class Destination
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public AddressDto Address { get; set; }
    }

    private class Address
    {
        public string City { get; set; }
        public string Country { get; set; }
    }

    private class AddressDto
    {
        public string City { get; set; }
        public string Country { get; set; }
    }

    #endregion

    #region Condition Tests

    [Fact]
    public void Condition_WhenTrue_ShouldMapProperty()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.Condition(s => s.IsActive);
            });

        var source = new Source { Name = "John", IsActive = true };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public void Condition_WhenFalse_ShouldNotMapProperty()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.Condition(s => s.IsActive);
            });

        var source = new Source { Name = "John", IsActive = false };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Name); // Should remain default (null)
    }

    [Fact]
    public void Condition_WithoutMapFrom_ShouldApplyToDefaultMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Email, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Email)));

        var sourceWithEmail = new Source { Email = "test@example.com" };
        var sourceWithoutEmail = new Source { Email = null };

        // Act
        var resultWithEmail = engine.MapInstance<Source, Destination>(sourceWithEmail);

        engine.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Email, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Email)));
        var resultWithoutEmail = engine.MapInstance<Source, Destination>(sourceWithoutEmail);

        // Assert
        Assert.Equal("test@example.com", resultWithEmail.Email);
        Assert.Null(resultWithoutEmail.Email);
    }

    [Fact]
    public void Condition_WithComplexExpression_ShouldWork()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name.ToUpper());
                opt.Condition(s => s.Name != null && s.Name.Length > 3);
            });

        var longName = new Source { Name = "Alexander" };
        var shortName = new Source { Name = "Jo" };

        // Act
        var resultLong = engine.MapInstance<Source, Destination>(longName);

        engine.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name.ToUpper());
                opt.Condition(s => s.Name != null && s.Name.Length > 3);
            });
        var resultShort = engine.MapInstance<Source, Destination>(shortName);

        // Assert
        Assert.Equal("ALEXANDER", resultLong.Name);
        Assert.Null(resultShort.Name);
    }

    #endregion

    #region NullSubstitute Tests

    [Fact]
    public void NullSubstitute_WhenSourceIsNull_ShouldUseSubstituteValue()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.NullSubstitute("N/A");
            });

        var source = new Source { Name = null };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("N/A", result.Name);
    }

    [Fact]
    public void NullSubstitute_WhenSourceIsNotNull_ShouldUseSourceValue()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.NullSubstitute("N/A");
            });

        var source = new Source { Name = "John" };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
    }

    [Fact]
    public void NullSubstitute_WithoutMapFrom_ShouldApplyToDefaultMapping()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Email, opt => opt.NullSubstitute("no-email@example.com"));

        var source = new Source { Email = null };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("no-email@example.com", result.Email);
    }

    [Fact]
    public void NullSubstitute_WithEmptyString_ShouldUseEmptyStringAsSubstitute()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.NullSubstitute("");
            });

        var source = new Source { Name = null };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("", result.Name);
    }

    #endregion

    #region Combined Condition and NullSubstitute Tests

    [Fact]
    public void Condition_And_NullSubstitute_WhenConditionTrue_ShouldApplyNullSubstitute()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.Condition(s => s.IsActive);
                opt.NullSubstitute("Unknown");
            });

        var source = new Source { Name = null, IsActive = true };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Equal("Unknown", result.Name);
    }

    [Fact]
    public void Condition_And_NullSubstitute_WhenConditionFalse_ShouldNotMap()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Name, opt =>
            {
                opt.MapFrom(s => s.Name);
                opt.Condition(s => s.IsActive);
                opt.NullSubstitute("Unknown");
            });

        var source = new Source { Name = null, IsActive = false };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Null(result.Name); // Condition is false, so no mapping occurs
    }

    #endregion

    #region Complex Type with Condition/NullSubstitute Tests

    [Fact]
    public void NullSubstitute_WithComplexType_WhenSourceIsNull_ShouldUseSubstitute()
    {
        // Arrange
        var engine = Mapper.Reset();
        var defaultAddress = new AddressDto { City = "Default City", Country = "Default Country" };

        engine.CreateMap<Address, AddressDto>();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Address, opt =>
            {
                opt.NullSubstitute(defaultAddress);
            });

        var source = new Source { Address = null };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result.Address);
        Assert.Equal("Default City", result.Address.City);
        Assert.Equal("Default Country", result.Address.Country);
    }

    [Fact]
    public void Condition_WithComplexType_WhenConditionFalse_ShouldNotMap()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Address, AddressDto>();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Address, opt =>
            {
                opt.Condition(s => s.IsActive);
            });

        var source = new Source
        {
            Address = new Address { City = "Seoul", Country = "Korea" },
            IsActive = false
        };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.Null(result.Address);
    }

    [Fact]
    public void Condition_WithComplexType_WhenConditionTrue_ShouldMap()
    {
        // Arrange
        var engine = Mapper.Reset();
        engine.CreateMap<Address, AddressDto>();
        engine.CreateMap<Source, Destination>()
            .ForMember(d => d.Address, opt =>
            {
                opt.Condition(s => s.IsActive);
            });

        var source = new Source
        {
            Address = new Address { City = "Seoul", Country = "Korea" },
            IsActive = true
        };

        // Act
        var result = engine.MapInstance<Source, Destination>(source);

        // Assert
        Assert.NotNull(result.Address);
        Assert.Equal("Seoul", result.Address.City);
    }

    #endregion
}
