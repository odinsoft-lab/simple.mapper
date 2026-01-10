using System.Diagnostics;
using Simple.AutoMapper.Tests.Models;
using Simple.AutoMapper.Tests.Models.DTO;
using System.Threading.Tasks;
using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class MappingEngineTests
    {
        private MappingEngine CreateConfiguredEngine()
        {

            var engine = Mapper.Reset();
            
            // Configure all mappings
            engine.CreateMap<Entity1, EntityDTO1>();
            engine.CreateMap<Entity2, EntityDTO2>();
            engine.CreateMap<Entity3, EntityDTO3>();
            engine.CreateMap<Entity4, EntityDTO4>();
            engine.CreateMap<Entity5, EntityDTO5>();
            engine.CreateMap<Entity6, EntityDTO6>();
            engine.CreateMap<Entity7, EntityDTO7>();
            engine.CreateMap<Entity8, EntityDTO8>();
            engine.CreateMap<Entity9, EntityDTO9>();
            engine.CreateMap<Entity10, EntityDTO10>();
            engine.CreateMap<Entity11, EntityDTO11>();
            engine.CreateMap<Entity12, EntityDTO12>();
            engine.CreateMap<Entity13, EntityDTO13>();
            engine.CreateMap<Entity14, EntityDTO14>();
            engine.CreateMap<Entity15, EntityDTO15>();
            engine.CreateMap<Entity16, EntityDTO16>();
            engine.CreateMap<Entity17, EntityDTO17>();
            engine.CreateMap<Entity18, EntityDTO18>();
            engine.CreateMap<Entity19, EntityDTO19>();
            engine.CreateMap<Entity20, EntityDTO20>();
            engine.CreateMap<Entity21, EntityDTO21>();
            engine.CreateMap<Entity22, EntityDTO22>();
            engine.CreateMap<Entity23, EntityDTO23>();
            engine.CreateMap<Entity24, EntityDTO24>();
            engine.CreateMap<Entity25, EntityDTO25>();
            engine.CreateMap<Entity26, EntityDTO26>();
            
            return engine;
        }

        [Fact]
        public void Map_Entity_ShouldMapAllProperties()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entity = new Entity8
            {
                Id = Guid.NewGuid()
            };

            // Act
            var dto = engine.MapInstance<Entity8, EntityDTO8>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void Map_ComplexEntity_ShouldMapNestedProperties()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var specialityId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var centerId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();

            var speciality = new Entity22 { Id = specialityId };
            var service = new Entity20 
            { 
                Id = serviceId,
                Entity22 = speciality,
                Entity22Id = specialityId
            };
            var patient = new Entity14 { Id = patientId };
            var resource = new Entity17 { Id = resourceId };
            var center = new Entity8 { Id = centerId };
            
            var entity = new Entity1
            {
                Id = appointmentId,
                Entity17Id = resourceId,
                Entity17 = resource,
                Entity22Id = specialityId,
                Entity22 = speciality,
                Entity20Id = serviceId,
                Entity20 = service,
                Entity14Id = patientId,
                Entity14 = patient,
                Entity8Id = centerId,
                Entity8 = center
            };

            // Act
            var dto = engine.MapInstance<Entity1, EntityDTO1>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Entity17Id, dto.Entity17Id);
            Assert.Equal(entity.Entity22Id, dto.Entity22Id);
            Assert.Equal(entity.Entity20Id, dto.Entity20Id);
            Assert.Equal(entity.Entity14Id, dto.Entity14Id);
            Assert.Equal(entity.Entity8Id, dto.Entity8Id);
            
            Assert.NotNull(dto.Entity17);
            Assert.Equal(resource.Id, dto.Entity17.Id);
            
            Assert.NotNull(dto.Entity22);
            Assert.Equal(speciality.Id, dto.Entity22.Id);
            
            Assert.NotNull(dto.Entity20);
            Assert.Equal(service.Id, dto.Entity20.Id);
            
            Assert.NotNull(dto.Entity14);
            Assert.Equal(patient.Id, dto.Entity14.Id);
            
            Assert.NotNull(dto.Entity8);
            Assert.Equal(center.Id, dto.Entity8.Id);
        }

        [Fact]
        public void Map_NullEntity_ShouldReturnNull()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            Entity1 entity = null;

            // Act
            var dto = engine.MapInstance<Entity1, EntityDTO1>(entity);

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public void MapList_MultipleEntities_ShouldMapAllItems()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entities = new List<Entity17>
            {
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() }
            };

            // Act
            var dtos = engine.MapCollection<Entity17, EntityDTO17>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(entities.Count, dtos.Count);
            
            for (int i = 0; i < entities.Count; i++)
            {
                Assert.Equal(entities[i].Id, dtos[i].Id);
            }
        }

        [Fact]
        public void MapList_NullList_ShouldReturnNull()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            List<Entity1> entities = null;

            // Act
            var dtos = engine.MapCollection<Entity1, EntityDTO1>(entities);

            // Assert
            Assert.Null(dtos);
        }

        [Fact]
        public void MapList_EmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entities = new List<Entity1>();

            // Act
            var dtos = engine.MapCollection<Entity1, EntityDTO1>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Empty(dtos);
        }

        [Fact]
        public void Map_WithNullableProperties_ShouldHandleNulls()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entity = new Entity1
            {
                Id = Guid.NewGuid(),
                Entity17Id = Guid.NewGuid(),
                Entity17 = new Entity17 { Id = Guid.NewGuid() },
                Entity22Id = null,  // Nullable property
                Entity22 = null,     // Null reference
                Entity20Id = null,   // Nullable property
                Entity20 = null,     // Null reference
                Entity12Id = null,   // Nullable property
                Entity12 = null,     // Null reference
                Entity14Id = Guid.NewGuid(),
                Entity14 = new Entity14 { Id = Guid.NewGuid() },
                Entity8Id = Guid.NewGuid(),
                Entity8 = new Entity8 { Id = Guid.NewGuid() }
            };

            // Act
            var dto = engine.MapInstance<Entity1, EntityDTO1>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Entity17Id, dto.Entity17Id);
            Assert.NotNull(dto.Entity17);
            Assert.Null(dto.Entity22Id);
            Assert.Null(dto.Entity22);
            Assert.Null(dto.Entity20Id);
            Assert.Null(dto.Entity20);
            Assert.Null(dto.Entity12Id);
            Assert.Null(dto.Entity12);
            Assert.Equal(entity.Entity14Id, dto.Entity14Id);
            Assert.NotNull(dto.Entity14);
            Assert.Equal(entity.Entity8Id, dto.Entity8Id);
            Assert.NotNull(dto.Entity8);
        }

        [Fact]
        public void Map_WithIgnoredProperty_ShouldNotMapIgnoredProperty()
        {
            // Arrange

            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>()
                .Ignore(d => d.Id); // Ignore Id property

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act
            var dto = engine.MapInstance<Entity17, EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(Guid.Empty, dto.Id); // Should be default value
        }

        [Fact]
        public void PerformanceTest_CompiledMapping_ShouldBeFasterThanReflection()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var initialize = new ModelTests.Initialize();
            var entities = new List<Entity1>();
            
            for (int i = 0; i < 1000; i++)
            {
                entities.Add(initialize.GenerateAppointment());
            }

            // Warmup
            _ = engine.MapInstance<Entity1, EntityDTO1>(entities[0]);

            // Act - First run (should compile and cache)
            var sw1 = Stopwatch.StartNew();
            var dtos1 = engine.MapCollection<Entity1, EntityDTO1>(entities);
            sw1.Stop();
            var firstRunTicks = sw1.ElapsedTicks;

            // Act - Second run (should use cached compiled mapping)
            var sw2 = Stopwatch.StartNew();
            var dtos2 = engine.MapCollection<Entity1, EntityDTO1>(entities);
            sw2.Stop();
            var secondRunTicks = sw2.ElapsedTicks;

            // Act - Third run (extra sample to reduce flakiness)
            var sw3 = Stopwatch.StartNew();
            var dtos3 = engine.MapCollection<Entity1, EntityDTO1>(entities);
            sw3.Stop();
            var thirdRunTicks = sw3.ElapsedTicks;

            // Assert
            Assert.NotNull(dtos1);
            Assert.NotNull(dtos2);
            Assert.NotNull(dtos3);
            Assert.Equal(entities.Count, dtos1.Count);
            Assert.Equal(entities.Count, dtos2.Count);
            Assert.Equal(entities.Count, dtos3.Count);
            
            // Second run should be faster or at least similar (due to caching)
            // We allow some tolerance due to system variations
            var bestOfLater = Math.Min(secondRunTicks, thirdRunTicks);
            Assert.True(bestOfLater <= firstRunTicks * 1.5, 
                $"A cached run should be faster than or similar to the first run: first={firstRunTicks} ticks, second={secondRunTicks} ticks, third={thirdRunTicks} ticks");

            // Both runs should complete in reasonable time
            var ticksPerMillisecond = Stopwatch.Frequency / 1000.0;
            Assert.True(firstRunTicks < (long)(5000 * ticksPerMillisecond), $"First run took {firstRunTicks} ticks, which is too slow");
            Assert.True(secondRunTicks < (long)(2000 * ticksPerMillisecond) || thirdRunTicks < (long)(2000 * ticksPerMillisecond), 
                $"Cached runs took too long: second={secondRunTicks} ticks, third={thirdRunTicks} ticks");
        }

        [Fact]
        public void ReverseMap_SimpleEntity_ShouldCreateBidirectionalMapping()
        {
            // Arrange

            var engine = Mapper.Reset();
            engine.CreateMap<Entity8, EntityDTO8>()
                .ReverseMap();

            var entity = new Entity8 { Id = Guid.NewGuid() };
            var dto = new EntityDTO8 { Id = Guid.NewGuid() };

            // Act - Forward mapping
            var mappedDto = engine.MapInstance<Entity8, EntityDTO8>(entity);
            
            // Act - Reverse mapping
            var mappedEntity = engine.MapInstance<EntityDTO8, Entity8>(dto);

            // Assert
            Assert.NotNull(mappedDto);
            Assert.Equal(entity.Id, mappedDto.Id);
            
            Assert.NotNull(mappedEntity);
            Assert.Equal(dto.Id, mappedEntity.Id);
        }

        [Fact]
        public void ReverseMap_ComplexEntity_ShouldMapNestedProperties()
        {
            // Arrange

            var engine = Mapper.Reset();
            
            // Configure all necessary mappings with ReverseMap
            engine.CreateMap<Entity22, EntityDTO22>().ReverseMap();
            engine.CreateMap<Entity20, EntityDTO20>().ReverseMap();
            engine.CreateMap<Entity14, EntityDTO14>().ReverseMap();
            engine.CreateMap<Entity17, EntityDTO17>().ReverseMap();
            engine.CreateMap<Entity8, EntityDTO8>().ReverseMap();
            engine.CreateMap<Entity1, EntityDTO1>().ReverseMap();

            var entity = new Entity1
            {
                Id = Guid.NewGuid(),
                Entity8Id = Guid.NewGuid(),
                Entity8 = new Entity8 { Id = Guid.NewGuid() }
            };

            // Act - Forward mapping
            var dto = engine.MapInstance<Entity1, EntityDTO1>(entity);
            
            // Act - Reverse mapping back to entity
            var reversedEntity = engine.MapInstance<EntityDTO1, Entity1>(dto);

            // Assert forward mapping
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Entity8Id, dto.Entity8Id);
            Assert.NotNull(dto.Entity8);
            Assert.Equal(entity.Entity8.Id, dto.Entity8.Id);
            
            // Assert reverse mapping
            Assert.NotNull(reversedEntity);
            Assert.Equal(dto.Id, reversedEntity.Id);
            Assert.Equal(dto.Entity8Id, reversedEntity.Entity8Id);
            Assert.NotNull(reversedEntity.Entity8);
            Assert.Equal(dto.Entity8.Id, reversedEntity.Entity8.Id);
        }

        [Fact]
        public void ReverseMap_WithIgnoredProperties_ShouldNotReverseIgnores()
        {
            // Arrange

            var engine = Mapper.Reset();
            var entityId = Guid.NewGuid();
            var manualId = Guid.NewGuid();
            
            // Configure mapping with ignored property, then reverse
            engine.CreateMap<Entity17, EntityDTO17>()
                .Ignore(d => d.Id)
                .ReverseMap();

            var entity = new Entity17
            { 
                Id = entityId
            };

            // Act - Forward mapping (Id should be ignored)
            var dto = engine.MapInstance<Entity17, EntityDTO17>(entity);
            
            // Verify that the ignored property wasn't mapped
            Assert.NotNull(dto);
            Assert.Equal(Guid.Empty, dto.Id); // Should be ignored in forward mapping
            
            // Set Id on DTO manually
            dto.Id = manualId;
            
            // Act - Reverse mapping (Id should be mapped normally)
            var reversedEntity = engine.MapInstance<EntityDTO17, Entity17>(dto);

            // Assert reverse mapping
            Assert.NotNull(reversedEntity);
            Assert.Equal(manualId, reversedEntity.Id); // Should be mapped in reverse
        }

        [Fact]
        public void CachingTest_SameTypeMapping_ShouldUseCachedVersion()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entity1 = new Entity25 { Id = Guid.NewGuid() };
            var entity2 = new Entity25 { Id = Guid.NewGuid() };

            // Act - First mapping (compiles)
            var sw1 = Stopwatch.StartNew();
            var dto1 = engine.MapInstance<Entity25, EntityDTO25>(entity1);
            sw1.Stop();

            // Act - Second mapping (uses cache)
            var sw2 = Stopwatch.StartNew();
            var dto2 = engine.MapInstance<Entity25, EntityDTO25>(entity2);
            sw2.Stop();

            // Assert
            Assert.NotNull(dto1);
            Assert.NotNull(dto2);
            Assert.Equal(entity1.Id, dto1.Id);
            Assert.Equal(entity2.Id, dto2.Id);
            
            // Second mapping should be significantly faster due to caching
            Assert.True(sw2.ElapsedTicks <= sw1.ElapsedTicks, 
                "Second mapping should be faster due to cached compiled expression");
        }

        [Fact]
        public async Task Map_MultipleMappingsSimultaneously_ShouldHandleConcurrency()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var tasks = new List<System.Threading.Tasks.Task<EntityDTO13>>();

            // Act - Create multiple mappings in parallel
            for (int i = 0; i < 10; i++)
            {
                var entity = new Entity13 { Id = Guid.NewGuid() };
                var task = System.Threading.Tasks.Task.Run(() => 
                    engine.MapInstance<Entity13, EntityDTO13>(entity));
                tasks.Add(task);
            }

            var results = await System.Threading.Tasks.Task.WhenAll(tasks);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(10, results.Length);
            Assert.All(results, dto => Assert.NotNull(dto));
            Assert.All(results, dto => Assert.NotEqual(Guid.Empty, dto.Id));
        }
    }
}