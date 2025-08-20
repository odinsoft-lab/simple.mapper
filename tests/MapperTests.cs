using Mapper.Tests.Models;
using Mapper.Tests.Models.DTO;

namespace Simple.AutoMapper.Tests
{
    public class MapperTests
    {
        [Fact]
        public void Map_Entity_ShouldMapAllProperties()
        {
            // Arrange
            var entity = new Entity8
            {
                Id = Guid.NewGuid()
            };

            // Act
            var dto = Mapper.Map<Entity8, EntityDTO8>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void Map_ComplexEntity_ShouldMapNestedProperties()
        {
            // Arrange
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
            var dto = Mapper.Map<Entity1, EntityDTO1>(entity);

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
            Entity1 entity = null;

            // Act
            var dto = Mapper.Map<Entity1, EntityDTO1>(entity);

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public void MapList_MultipleEntities_ShouldMapAllItems()
        {
            // Arrange
            var entities = new List<Entity17>
            {
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() }
            };

            // Act
            var dtos = Mapper.MapList<Entity17, EntityDTO17>(entities);

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
            List<Entity1> entities = null;

            // Act
            var dtos = Mapper.MapList<Entity1, EntityDTO1>(entities);

            // Assert
            Assert.Null(dtos);
        }

        [Fact]
        public void MapList_EmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var entities = new List<Entity1>();

            // Act
            var dtos = Mapper.MapList<Entity1, EntityDTO1>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Empty(dtos);
        }

        // [Fact]
        // public void ExtensionMethod_MapTo_ShouldWork()
        // {
        //     // Arrange
        //     var entity = new Entity25 { Id = Guid.NewGuid() };

        //     // Act
        //     var dto = entity.MapTo<EntityDTO25>();

        //     // Assert
        //     Assert.NotNull(dto);
        //     Assert.Equal(entity.Id, dto.Id);
        // }

        // [Fact]
        // public void ExtensionMethod_MapToList_ShouldWork()
        // {
        //     // Arrange
        //     var entities = new List<Entity13>
        //     {
        //         new Entity13 { Id = Guid.NewGuid() },
        //         new Entity13 { Id = Guid.NewGuid() }
        //     };

        //     // Act
        //     var dtos = entities.MapToList<EntityDTO13>();

        //     // Assert
        //     Assert.NotNull(dtos);
        //     Assert.Equal(entities.Count, dtos.Count);
            
        //     for (int i = 0; i < entities.Count; i++)
        //     {
        //         Assert.Equal(entities[i].Id, dtos[i].Id);
        //     }
        // }

        [Fact]
        public void Map_WithNullableProperties_ShouldHandleNulls()
        {
            // Arrange
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
            var dto = Mapper.Map<Entity1, EntityDTO1>(entity);

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
        public void PerformanceComparison_AutoMapper_ShouldBeEfficient()
        {
            // Arrange
            var initialize = new ModelTests.Initialize();
            var entities = new List<Entity1>();
            
            for (int i = 0; i < 100; i++)
            {
                entities.Add(initialize.GenerateAppointment());
            }

            // Act
            var startTime = DateTime.Now;
            var dtos = Mapper.MapList<Entity1, EntityDTO1>(entities);
            var endTime = DateTime.Now;
            
            var mappingTime = (endTime - startTime).TotalMilliseconds;

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(entities.Count, dtos.Count);
            
            // Verify first item is mapped correctly
            var firstEntity = entities[0];
            var firstDto = dtos[0];
            
            Assert.Equal(firstEntity.Id, firstDto.Id);
            Assert.Equal(firstEntity.Entity17Id, firstDto.Entity17Id);
            Assert.Equal(firstEntity.Entity22Id, firstDto.Entity22Id);
            Assert.Equal(firstEntity.Entity20Id, firstDto.Entity20Id);
            Assert.Equal(firstEntity.Entity14Id, firstDto.Entity14Id);
            Assert.Equal(firstEntity.Entity8Id, firstDto.Entity8Id);
            
            // Performance should be reasonable (less than 1 second for 100 items)
            Assert.True(mappingTime < 1000, $"Mapping took {mappingTime}ms, which is too slow");
        }
    }
}