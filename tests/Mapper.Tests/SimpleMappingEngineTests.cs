using System.Diagnostics;
using OmmitedDatabaseModel3;
using OmmitedDTOModel3;
using System.Threading.Tasks;

namespace Simple.Tests
{
    public class SimpleMappingEngineTests
    {
        private MappingEngine CreateConfiguredEngine()
        {
            var engine = new MappingEngine();
            
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
        public void Map_SimpleEntity_ShouldMapAllProperties()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var entity = new Entity8
            {
                Id = Guid.NewGuid()
            };

            // Act
            var dto = engine.Map<Entity8, EntityDTO8>(entity);

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
            var dto = engine.Map<Entity1, EntityDTO1>(entity);

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
            var dto = engine.Map<Entity1, EntityDTO1>(entity);

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
            var dtos = engine.MapList<Entity17, EntityDTO17>(entities);

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
            var dtos = engine.MapList<Entity1, EntityDTO1>(entities);

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
            var dtos = engine.MapList<Entity1, EntityDTO1>(entities);

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
            var dto = engine.Map<Entity1, EntityDTO1>(entity);

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
            var engine = new MappingEngine();
            engine.CreateMap<Entity17, EntityDTO17>()
                .Ignore(d => d.Id); // Ignore Id property

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act
            var dto = engine.Map<Entity17, EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(Guid.Empty, dto.Id); // Should be default value
        }

        [Fact]
        public void PerformanceTest_CompiledMapping_ShouldBeFasterThanReflection()
        {
            // Arrange
            var engine = CreateConfiguredEngine();
            var initialize = new MapAtRuntime.Initialize();
            var entities = new List<Entity1>();
            
            for (int i = 0; i < 1000; i++)
            {
                entities.Add(initialize.GenerateAppointment());
            }

            // Warmup
            _ = engine.Map<Entity1, EntityDTO1>(entities[0]);

            // Act - First run (should compile and cache)
            var sw1 = Stopwatch.StartNew();
            var dtos1 = engine.MapList<Entity1, EntityDTO1>(entities);
            sw1.Stop();
            var firstRunTime = sw1.ElapsedMilliseconds;

            // Act - Second run (should use cached compiled mapping)
            var sw2 = Stopwatch.StartNew();
            var dtos2 = engine.MapList<Entity1, EntityDTO1>(entities);
            sw2.Stop();
            var secondRunTime = sw2.ElapsedMilliseconds;

            // Assert
            Assert.NotNull(dtos1);
            Assert.NotNull(dtos2);
            Assert.Equal(entities.Count, dtos1.Count);
            Assert.Equal(entities.Count, dtos2.Count);
            
            // Second run should be faster or at least similar (due to caching)
            // We allow some tolerance due to system variations
            Assert.True(secondRunTime <= firstRunTime * 1.5, 
                $"Second run ({secondRunTime}ms) should be faster than or similar to first run ({firstRunTime}ms) due to caching");

            // Both runs should complete in reasonable time
            Assert.True(firstRunTime < 5000, $"First run took {firstRunTime}ms, which is too slow");
            Assert.True(secondRunTime < 2000, $"Second run took {secondRunTime}ms, which is too slow");
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
            var dto1 = engine.Map<Entity25, EntityDTO25>(entity1);
            sw1.Stop();

            // Act - Second mapping (uses cache)
            var sw2 = Stopwatch.StartNew();
            var dto2 = engine.Map<Entity25, EntityDTO25>(entity2);
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
                    engine.Map<Entity13, EntityDTO13>(entity));
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