using Microsoft.Extensions.DependencyInjection;
using Simple.AutoMapper.Core;
using Simple.AutoMapper.DependencyInjection;
using Simple.AutoMapper.Interfaces;
using Simple.AutoMapper.Tests.Models;
using Simple.AutoMapper.Tests.Models.DTO;
using SimpleMapperConfig = Simple.AutoMapper.DependencyInjection.MapperConfiguration;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddSimpleMapper_WithConfiguration_ShouldRegisterServices()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();

            // Act
            services.AddSimpleMapper(config =>
            {
                config.AddProfile<TestProfile>();
            });

            var provider = services.BuildServiceProvider();
            var mapper = provider.GetService<ISimpleMapper>();

            // Assert
            Assert.NotNull(mapper);
        }

        [Fact]
        public void AddSimpleMapper_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services.AddSimpleMapper((Action<SimpleMapperConfig>)null!));
        }

        [Fact]
        public void AddSimpleMapper_WithAssembly_ShouldRegisterServices()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();

            // Act
            services.AddSimpleMapper(typeof(TestProfile).Assembly);

            var provider = services.BuildServiceProvider();
            var mapper = provider.GetService<ISimpleMapper>();

            // Assert
            Assert.NotNull(mapper);
        }

        [Fact]
        public void AddSimpleMapper_WithNullAssemblies_ShouldThrowArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                services.AddSimpleMapper(assemblies: null));
        }

        [Fact]
        public void AddSimpleMapper_WithEmptyAssemblies_ShouldThrowArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                services.AddSimpleMapper(new System.Reflection.Assembly[0]));
        }

        [Fact]
        public void MapperConfiguration_AddProfile_Generic_ShouldConfigureMapping()
        {
            // Arrange
            Mapper.Reset();
            var config = new SimpleMapperConfig();

            // Act
            var result = config.AddProfile<TestProfile>();

            // Assert
            Assert.Same(config, result); // Should return same instance for chaining

            // Verify mapping works
            var entity = new Entity8 { Id = Guid.NewGuid() };
            var dto = Mapper.Map<Entity8, EntityDTO8>(entity);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void MapperConfiguration_AddProfile_Instance_ShouldConfigureMapping()
        {
            // Arrange
            Mapper.Reset();
            var config = new SimpleMapperConfig();
            var profile = new TestProfile();

            // Act
            var result = config.AddProfile(profile);

            // Assert
            Assert.Same(config, result); // Should return same instance for chaining

            // Verify mapping works
            var entity = new Entity8 { Id = Guid.NewGuid() };
            var dto = Mapper.Map<Entity8, EntityDTO8>(entity);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void ISimpleMapper_Map_SingleItem_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var entity = new Entity8 { Id = Guid.NewGuid() };

            // Act
            var dto = mapper.Map<Entity8, EntityDTO8>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void ISimpleMapper_Map_WithInferredType_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var entity = new Entity8 { Id = Guid.NewGuid() };

            // Act
            var dto = mapper.Map<EntityDTO8>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void ISimpleMapper_Map_Collection_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var entities = new List<Entity8>
            {
                new Entity8 { Id = Guid.NewGuid() },
                new Entity8 { Id = Guid.NewGuid() }
            };

            // Act
            var dtos = mapper.Map<Entity8, EntityDTO8>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(2, dtos.Count);
            Assert.Equal(entities[0].Id, dtos[0].Id);
            Assert.Equal(entities[1].Id, dtos[1].Id);
        }

        [Fact]
        public void ISimpleMapper_Map_InPlace_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var entity = new Entity8 { Id = Guid.NewGuid() };
            var dto = new EntityDTO8 { Id = Guid.Empty };

            // Act
            mapper.Map(entity, dto);

            // Assert
            Assert.Equal(entity.Id, dto.Id);
        }
    }

    // Test Profile for DI tests
    public class TestProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity8, EntityDTO8>();
            CreateMap<Entity14, EntityDTO14>();
            CreateMap<Entity17, EntityDTO17>();
        }
    }
}
