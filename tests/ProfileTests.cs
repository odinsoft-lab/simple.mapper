using Simple.AutoMapper.Core;
using Simple.AutoMapper.Tests.Models;
using Simple.AutoMapper.Tests.Models.DTO;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class ProfileTests
    {
        [Fact]
        public void AddProfile_Generic_ShouldExecuteConfiguration()
        {
            // Arrange
            Mapper.Reset();

            // Act
            Mapper.AddProfile<SimpleTestProfile>();

            // Assert - verify mapping works
            var entity = new Entity17 { Id = Guid.NewGuid() };
            var dto = Mapper.Map<Entity17, EntityDTO17>(entity);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void AddProfile_Instance_ShouldExecuteConfiguration()
        {
            // Arrange
            Mapper.Reset();
            var profile = new SimpleTestProfile();

            // Act
            Mapper.AddProfile(profile);

            // Assert - verify mapping works
            var entity = new Entity17 { Id = Guid.NewGuid() };
            var dto = Mapper.Map<Entity17, EntityDTO17>(entity);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void AddProfile_WithNullProfile_ShouldThrowArgumentNullException()
        {
            // Arrange
            Mapper.Reset();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Mapper.AddProfile(null));
        }

        [Fact]
        public void Profile_CreateMap_ShouldDelegateToMapper()
        {
            // Arrange
            Mapper.Reset();

            // Act
            Mapper.AddProfile<ProfileWithIgnore>();

            // Assert - verify ignored property is not mapped
            var entity = new Entity8 { Id = Guid.NewGuid() };
            var dto = Mapper.Map<Entity8, EntityDTO8>(entity);

            Assert.NotNull(dto);
            Assert.Equal(Guid.Empty, dto.Id); // Id should be ignored
        }

        [Fact]
        public void AddProfile_MultipleProfiles_ShouldAllWork()
        {
            // Arrange
            Mapper.Reset();

            // Act
            Mapper.AddProfile<ProfileForEntity8>();
            Mapper.AddProfile<ProfileForEntity14>();

            // Assert
            var entity8 = new Entity8 { Id = Guid.NewGuid() };
            var dto8 = Mapper.Map<Entity8, EntityDTO8>(entity8);
            Assert.Equal(entity8.Id, dto8.Id);

            var entity14 = new Entity14 { Id = Guid.NewGuid() };
            var dto14 = Mapper.Map<Entity14, EntityDTO14>(entity14);
            Assert.Equal(entity14.Id, dto14.Id);
        }
    }

    public class SimpleTestProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity17, EntityDTO17>();
        }
    }

    public class ProfileWithIgnore : Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity8, EntityDTO8>()
                .Ignore(d => d.Id);
        }
    }

    public class ProfileForEntity8 : Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity8, EntityDTO8>();
        }
    }

    public class ProfileForEntity14 : Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity14, EntityDTO14>();
        }
    }
}
