using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests;

[Collection("Mapper Tests")]
public class MapFromTests
{
    private class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
    }

    private class Address
    {
        public string City { get; set; }
    }

    private class PersonDto
    {
        public string Name { get; set; }
        public int NameLength { get; set; }
        public string City { get; set; }
    }

    private class Team
    {
        public List<Person> Members { get; set; }
    }

    private class TeamDto
    {
        public List<PersonDto> Members { get; set; }
    }

    [Fact]
    public void MapFrom_SimpleProperty_MapsFromDifferentSourceProperty()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName));

        var dto = engine.MapInstance<Person, PersonDto>(new Person { FirstName = "John" });

        Assert.NotNull(dto);
        Assert.Equal("John", dto.Name);
    }

    [Fact]
    public void MapFrom_Expression_ComputedValue_IsAssigned()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .ForMember(d => d.NameLength, opt => opt.MapFrom(s => s.FirstName.Length));

        var dto = engine.MapInstance<Person, PersonDto>(new Person { FirstName = "Alex" });

        Assert.NotNull(dto);
        Assert.Equal(4, dto.NameLength);
    }

    [Fact]
    public void MapFrom_NestedProperty_MapsNestedToFlat()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Address.City));

        var dto = engine.MapInstance<Person, PersonDto>(new Person { Address = new Address { City = "Seoul" } });

        Assert.NotNull(dto);
        Assert.Equal("Seoul", dto.City);
    }

    [Fact]
    public void MapFrom_WithIgnore_IgnoreWins_EvenIfBothConfigured()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName))
            .Ignore(d => d.Name);

        var dto = engine.MapInstance<Person, PersonDto>(new Person { FirstName = "Jane" });

        Assert.NotNull(dto);
        Assert.Null(dto.Name);
    }

    [Fact]
    public void MapFrom_WithIgnore_IgnoreWins_RegardlessOfOrder()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .Ignore(d => d.Name)
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName));

        var dto = engine.MapInstance<Person, PersonDto>(new Person { FirstName = "Jane" });

        Assert.NotNull(dto);
        Assert.Null(dto.Name);
    }

    [Fact]
    public void MapFrom_Collections_MinimalMapping_Works()
    {
        var engine = Mapper.Reset();
        engine.CreateMap<Person, PersonDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FirstName));
        engine.CreateMap<Team, TeamDto>()
            .ForMember(d => d.Members, opt => opt.MapFrom(s => s.Members == null
                ? null
                : s.Members.Select(x => new PersonDto { Name = x.FirstName }).ToList()));

        var team = new Team
        {
            Members = new List<Person>
            {
                new Person { FirstName = "A" },
                new Person { FirstName = "B" }
            }
        };

        var dto = engine.MapInstance<Team, TeamDto>(team);

        Assert.NotNull(dto);
        Assert.NotNull(dto.Members);
        Assert.Equal(2, dto.Members.Count);
        Assert.Equal("A", dto.Members[0].Name);
        Assert.Equal("B", dto.Members[1].Name);
    }
}
