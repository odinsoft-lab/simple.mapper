using Microsoft.Extensions.DependencyInjection;
using Simple.AutoMapper.Core;
using Simple.AutoMapper.DependencyInjection;
using Simple.AutoMapper.Interfaces;

namespace Simple.AutoMapper.Tests
{
    #region Test Models

    public enum PatchTestStatus { Active, Inactive, Pending }

    // Entity with non-nullable properties (destination)
    public class PatchTestEntity
    {
        public string Name { get; set; } = "Original";
        public string Email { get; set; } = "original@test.com";
        public int Age { get; set; } = 25;
        public DateTime CreatedDate { get; set; } = new DateTime(2024, 1, 1);
        public bool IsActive { get; set; } = true;
        public PatchTestStatus Status { get; set; } = PatchTestStatus.Active;
    }

    // UpdateDto with nullable properties for partial updates (source)
    public class PatchTestUpdateDto
    {
        public string Name { get; set; }       // null = don't update
        public string Email { get; set; }      // null = don't update
        public int? Age { get; set; }          // null = don't update
        public DateTime? CreatedDate { get; set; }  // null = don't update
        public bool? IsActive { get; set; }    // null = don't update
        public PatchTestStatus? Status { get; set; } // null = don't update
    }

    // Nested complex type for testing
    public class PatchTestAddress
    {
        public string City { get; set; } = "DefaultCity";
        public string State { get; set; } = "DefaultState";
        public string ZipCode { get; set; } = "00000";
    }

    public class PatchTestAddressDto
    {
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    // Entity with nested object and collection
    public class PatchTestComplexEntity
    {
        public string Name { get; set; } = "ComplexOriginal";
        public PatchTestAddress Address { get; set; } = new PatchTestAddress
        {
            City = "OldCity",
            State = "CA",
            ZipCode = "90001"
        };
        public List<string> Tags { get; set; } = new List<string> { "tag1", "tag2" };
    }

    public class PatchTestComplexDto
    {
        public string Name { get; set; }
        public PatchTestAddressDto Address { get; set; }
        public List<string> Tags { get; set; }
    }

    // Entity and DTO with matching nullable types on both sides
    public class PatchTestNullableEntity
    {
        public int? Score { get; set; } = 100;
        public DateTime? LastLogin { get; set; } = new DateTime(2025, 6, 15);
        public bool? OptIn { get; set; } = true;
        public PatchTestStatus? Status { get; set; } = PatchTestStatus.Active;
    }

    public class PatchTestNullableDto
    {
        public int? Score { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool? OptIn { get; set; }
        public PatchTestStatus? Status { get; set; }
    }

    #endregion

    [Collection("Mapper Tests")]
    public class PatchTests
    {
        public PatchTests()
        {
            Mapper.Reset();
        }

        #region String properties

        [Fact]
        public void Patch_NullStringSource_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Name = null, Email = null };
            var entity = new PatchTestEntity { Name = "Original", Email = "original@test.com" };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("Original", entity.Name);
            Assert.Equal("original@test.com", entity.Email);
        }

        [Fact]
        public void Patch_NonNullStringSource_ShouldUpdateDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Name = "Updated", Email = "updated@test.com" };
            var entity = new PatchTestEntity { Name = "Original", Email = "original@test.com" };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("Updated", entity.Name);
            Assert.Equal("updated@test.com", entity.Email);
        }

        [Fact]
        public void Patch_MixedStringValues_ShouldOnlyUpdateNonNull()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Name = "Updated", Email = null };
            var entity = new PatchTestEntity { Name = "Original", Email = "original@test.com" };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("Updated", entity.Name);
            Assert.Equal("original@test.com", entity.Email); // preserved
        }

        #endregion

        #region Nullable value types

        [Fact]
        public void Patch_NullNullableInt_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Age = null };
            var entity = new PatchTestEntity { Age = 25 };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(25, entity.Age);
        }

        [Fact]
        public void Patch_NonNullNullableInt_ShouldUpdateDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Age = 30 };
            var entity = new PatchTestEntity { Age = 25 };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(30, entity.Age);
        }

        [Fact]
        public void Patch_NullNullableDateTime_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { CreatedDate = null };
            var entity = new PatchTestEntity { CreatedDate = new DateTime(2024, 1, 1) };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(new DateTime(2024, 1, 1), entity.CreatedDate);
        }

        [Fact]
        public void Patch_NonNullNullableDateTime_ShouldUpdateDestination()
        {
            // Arrange
            var expected = new DateTime(2026, 6, 15);
            var dto = new PatchTestUpdateDto { CreatedDate = expected };
            var entity = new PatchTestEntity { CreatedDate = new DateTime(2024, 1, 1) };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(expected, entity.CreatedDate);
        }

        [Fact]
        public void Patch_NullNullableBool_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { IsActive = null };
            var entity = new PatchTestEntity { IsActive = true };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.True(entity.IsActive);
        }

        [Fact]
        public void Patch_NonNullNullableBool_ShouldUpdateDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { IsActive = false };
            var entity = new PatchTestEntity { IsActive = true };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.False(entity.IsActive);
        }

        [Fact]
        public void Patch_NullNullableEnum_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Status = null };
            var entity = new PatchTestEntity { Status = PatchTestStatus.Active };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(PatchTestStatus.Active, entity.Status);
        }

        [Fact]
        public void Patch_NonNullNullableEnum_ShouldUpdateDestination()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Status = PatchTestStatus.Inactive };
            var entity = new PatchTestEntity { Status = PatchTestStatus.Active };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(PatchTestStatus.Inactive, entity.Status);
        }

        #endregion

        #region Nullable<T> source to Nullable<T> destination

        [Fact]
        public void Patch_NullableToNullable_NullSkipped()
        {
            // Arrange
            var dto = new PatchTestNullableDto { Score = null, LastLogin = null, OptIn = null, Status = null };
            var entity = new PatchTestNullableEntity
            {
                Score = 100,
                LastLogin = new DateTime(2025, 6, 15),
                OptIn = true,
                Status = PatchTestStatus.Active
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(100, entity.Score);
            Assert.Equal(new DateTime(2025, 6, 15), entity.LastLogin);
            Assert.True(entity.OptIn);
            Assert.Equal(PatchTestStatus.Active, entity.Status);
        }

        [Fact]
        public void Patch_NullableToNullable_NonNullApplied()
        {
            // Arrange
            var dto = new PatchTestNullableDto
            {
                Score = 200,
                LastLogin = new DateTime(2026, 1, 1),
                OptIn = false,
                Status = PatchTestStatus.Pending
            };
            var entity = new PatchTestNullableEntity
            {
                Score = 100,
                LastLogin = new DateTime(2025, 6, 15),
                OptIn = true,
                Status = PatchTestStatus.Active
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal(200, entity.Score);
            Assert.Equal(new DateTime(2026, 1, 1), entity.LastLogin);
            Assert.False(entity.OptIn);
            Assert.Equal(PatchTestStatus.Pending, entity.Status);
        }

        #endregion

        #region Non-nullable value types (always assigned)

        [Fact]
        public void Patch_NonNullableValueTypes_AlwaysAssigned()
        {
            // Non-nullable value types on source are always assigned (even if default)
            // Arrange - source has non-nullable int, bool, DateTime, enum with defaults
            var source = new PatchTestEntity
            {
                Name = "NewName",
                Email = "new@test.com",
                Age = 0,            // default int
                IsActive = false,   // default bool
                CreatedDate = default(DateTime),
                Status = PatchTestStatus.Active // first enum value
            };
            var dest = new PatchTestEntity
            {
                Name = "Original",
                Email = "original@test.com",
                Age = 50,
                IsActive = true,
                CreatedDate = new DateTime(2025, 1, 1),
                Status = PatchTestStatus.Inactive
            };

            // Act
            Mapper.Patch(source, dest);

            // Assert - all values assigned even defaults
            Assert.Equal("NewName", dest.Name);
            Assert.Equal("new@test.com", dest.Email);
            Assert.Equal(0, dest.Age);
            Assert.False(dest.IsActive);
            Assert.Equal(default(DateTime), dest.CreatedDate);
            Assert.Equal(PatchTestStatus.Active, dest.Status);
        }

        #endregion

        #region Mixed partial update scenario

        [Fact]
        public void Patch_MixedPartialUpdate_ShouldOnlyUpdateProvidedFields()
        {
            // Arrange - real-world scenario: only update Name and Status
            var dto = new PatchTestUpdateDto
            {
                Name = "Seoul Office",
                Email = null,          // skip
                Age = null,            // skip
                CreatedDate = null,    // skip
                IsActive = null,       // skip
                Status = PatchTestStatus.Pending
            };

            var entity = new PatchTestEntity
            {
                Name = "Original Office",
                Email = "office@test.com",
                Age = 5,
                CreatedDate = new DateTime(2020, 3, 15),
                IsActive = true,
                Status = PatchTestStatus.Active
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("Seoul Office", entity.Name);         // updated
            Assert.Equal("office@test.com", entity.Email);      // preserved
            Assert.Equal(5, entity.Age);                         // preserved
            Assert.Equal(new DateTime(2020, 3, 15), entity.CreatedDate); // preserved
            Assert.True(entity.IsActive);                        // preserved
            Assert.Equal(PatchTestStatus.Pending, entity.Status); // updated
        }

        #endregion

        #region Null guards

        [Fact]
        public void Patch_NullSource_ShouldNotThrow()
        {
            // Arrange
            var entity = new PatchTestEntity();

            // Act - should not throw
            Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(null, entity);

            // Assert - entity unchanged
            Assert.Equal("Original", entity.Name);
            Assert.Equal(25, entity.Age);
        }

        [Fact]
        public void Patch_NullDestination_ShouldNotThrow()
        {
            // Arrange
            var dto = new PatchTestUpdateDto { Name = "Test" };

            // Act - should not throw
            Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dto, null);
        }

        [Fact]
        public void Patch_BothNull_ShouldNotThrow()
        {
            // Act - should not throw
            Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(null, null);
        }

        #endregion

        #region Complex nested objects

        [Fact]
        public void Patch_NullComplexProperty_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestComplexDto { Name = null, Address = null, Tags = null };
            var entity = new PatchTestComplexEntity
            {
                Name = "ComplexOriginal",
                Address = new PatchTestAddress { City = "OldCity", State = "CA", ZipCode = "90001" },
                Tags = new List<string> { "tag1", "tag2" }
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert - all preserved
            Assert.Equal("ComplexOriginal", entity.Name);
            Assert.NotNull(entity.Address);
            Assert.Equal("OldCity", entity.Address.City);
            Assert.Equal("CA", entity.Address.State);
            Assert.Equal("90001", entity.Address.ZipCode);
            Assert.Equal(2, entity.Tags.Count);
        }

        [Fact]
        public void Patch_NonNullComplexProperty_ShouldMapNewValue()
        {
            // Arrange
            var dto = new PatchTestComplexDto
            {
                Name = "Updated",
                Address = new PatchTestAddressDto { City = "Seoul", State = "Seoul", ZipCode = "12345" }
            };
            var entity = new PatchTestComplexEntity
            {
                Name = "ComplexOriginal",
                Address = new PatchTestAddress { City = "OldCity", State = "CA", ZipCode = "90001" }
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("Updated", entity.Name);
            Assert.NotNull(entity.Address);
            Assert.Equal("Seoul", entity.Address.City);
            Assert.Equal("Seoul", entity.Address.State);
            Assert.Equal("12345", entity.Address.ZipCode);
        }

        #endregion

        #region Collection properties

        [Fact]
        public void Patch_NullCollectionProperty_ShouldPreserveDestination()
        {
            // Arrange
            var dto = new PatchTestComplexDto { Tags = null };
            var entity = new PatchTestComplexEntity
            {
                Tags = new List<string> { "existing1", "existing2" }
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.NotNull(entity.Tags);
            Assert.Equal(2, entity.Tags.Count);
            Assert.Equal("existing1", entity.Tags[0]);
        }

        [Fact]
        public void Patch_NonNullCollectionProperty_ShouldMapNewValue()
        {
            // Arrange
            var dto = new PatchTestComplexDto
            {
                Tags = new List<string> { "newTag1", "newTag2", "newTag3" }
            };
            var entity = new PatchTestComplexEntity
            {
                Tags = new List<string> { "existing1" }
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.NotNull(entity.Tags);
            Assert.Equal(3, entity.Tags.Count);
            Assert.Equal("newTag1", entity.Tags[0]);
        }

        #endregion

        #region DI interface (ISimpleMapper.Patch)

        [Fact]
        public void Patch_ViaISimpleMapper_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var dto = new PatchTestUpdateDto
            {
                Name = "DI Updated",
                Email = null,
                Age = 42,
                IsActive = null
            };

            var entity = new PatchTestEntity
            {
                Name = "Original",
                Email = "keep@test.com",
                Age = 25,
                IsActive = true
            };

            // Act
            mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("DI Updated", entity.Name);       // updated
            Assert.Equal("keep@test.com", entity.Email);    // preserved
            Assert.Equal(42, entity.Age);                    // updated
            Assert.True(entity.IsActive);                    // preserved
        }

        #endregion

        #region Edge cases

        [Fact]
        public void Patch_EmptyStringSource_ShouldUpdateDestination()
        {
            // Empty string is NOT null, so it should be applied
            // Arrange
            var dto = new PatchTestUpdateDto { Name = "" };
            var entity = new PatchTestEntity { Name = "Original" };

            // Act
            Mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("", entity.Name);
        }

        [Fact]
        public void Patch_AllNullDto_ShouldPreserveAllDestinationValues()
        {
            // Arrange
            var dto = new PatchTestUpdateDto(); // all null
            var entity = new PatchTestEntity
            {
                Name = "Keep",
                Email = "keep@test.com",
                Age = 30,
                CreatedDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Status = PatchTestStatus.Inactive
            };

            // Act
            Mapper.Patch(dto, entity);

            // Assert - everything preserved
            Assert.Equal("Keep", entity.Name);
            Assert.Equal("keep@test.com", entity.Email);
            Assert.Equal(30, entity.Age);
            Assert.Equal(new DateTime(2025, 1, 1), entity.CreatedDate);
            Assert.True(entity.IsActive);
            Assert.Equal(PatchTestStatus.Inactive, entity.Status);
        }

        [Fact]
        public void Patch_AllFieldsProvided_ShouldUpdateAllDestinationValues()
        {
            // Arrange
            var dto = new PatchTestUpdateDto
            {
                Name = "AllNew",
                Email = "allnew@test.com",
                Age = 99,
                CreatedDate = new DateTime(2026, 12, 31),
                IsActive = false,
                Status = PatchTestStatus.Pending
            };
            var entity = new PatchTestEntity();

            // Act
            Mapper.Patch(dto, entity);

            // Assert - everything updated
            Assert.Equal("AllNew", entity.Name);
            Assert.Equal("allnew@test.com", entity.Email);
            Assert.Equal(99, entity.Age);
            Assert.Equal(new DateTime(2026, 12, 31), entity.CreatedDate);
            Assert.False(entity.IsActive);
            Assert.Equal(PatchTestStatus.Pending, entity.Status);
        }

        #endregion

        #region Patch<TS,TD>(TS source) — new object

        [Fact]
        public void Patch_NewObject_ShouldCreateDestinationWithNonNullValues()
        {
            // Arrange
            var dto = new PatchTestUpdateDto
            {
                Name = "NewName",
                Email = null,
                Age = 30,
                IsActive = null
            };

            // Act
            var entity = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("NewName", entity.Name);           // updated
            Assert.Equal("original@test.com", entity.Email); // null skipped → constructor default preserved
            Assert.Equal(30, entity.Age);                     // updated
            Assert.True(entity.IsActive);                     // null skipped → constructor default preserved
        }

        [Fact]
        public void Patch_NewObject_NullSource_ShouldReturnDefault()
        {
            // Act
            PatchTestUpdateDto nullDto = null;
            var result = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(nullDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Patch_NewObject_AllNullDto_ShouldPreserveConstructorDefaults()
        {
            // Arrange
            var dto = new PatchTestUpdateDto(); // all null

            // Act
            var entity = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dto);

            // Assert - all null source properties skipped, constructor defaults preserved
            Assert.NotNull(entity);
            Assert.Equal("Original", entity.Name);
            Assert.Equal("original@test.com", entity.Email);
            Assert.Equal(25, entity.Age);
            Assert.True(entity.IsActive);
        }

        #endregion

        #region Patch<TD>(object source) — type-inferred new object

        [Fact]
        public void Patch_TypeInferred_ShouldCreateDestinationWithNonNullValues()
        {
            // Arrange
            var dto = new PatchTestUpdateDto
            {
                Name = "TypeInferred",
                Email = "test@inferred.com",
                Age = null,
                IsActive = true
            };

            // Act
            var entity = Mapper.Patch<PatchTestEntity>((object)dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("TypeInferred", entity.Name);
            Assert.Equal("test@inferred.com", entity.Email);
            Assert.Equal(25, entity.Age);    // null skipped → constructor default preserved
            Assert.True(entity.IsActive);
        }

        [Fact]
        public void Patch_TypeInferred_NullSource_ShouldReturnDefault()
        {
            // Act
            var result = Mapper.Patch<PatchTestEntity>(null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Patch<TS,TD>(IEnumerable<TS>) — collection

        [Fact]
        public void Patch_Collection_ShouldPatchEachItem()
        {
            // Arrange
            var dtos = new List<PatchTestUpdateDto>
            {
                new PatchTestUpdateDto { Name = "First", Email = null, Age = 10 },
                new PatchTestUpdateDto { Name = null, Email = "second@test.com", Age = 20 },
                new PatchTestUpdateDto { Name = "Third", Email = "third@test.com", Age = null }
            };

            // Act
            var entities = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dtos);

            // Assert
            Assert.NotNull(entities);
            Assert.Equal(3, entities.Count);

            Assert.Equal("First", entities[0].Name);
            Assert.Equal("original@test.com", entities[0].Email);  // null skipped → constructor default
            Assert.Equal(10, entities[0].Age);

            Assert.Equal("Original", entities[1].Name);            // null skipped → constructor default
            Assert.Equal("second@test.com", entities[1].Email);
            Assert.Equal(20, entities[1].Age);

            Assert.Equal("Third", entities[2].Name);
            Assert.Equal("third@test.com", entities[2].Email);
            Assert.Equal(25, entities[2].Age);  // null skipped → constructor default
        }

        [Fact]
        public void Patch_Collection_NullList_ShouldReturnNull()
        {
            // Act
            var result = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>((IEnumerable<PatchTestUpdateDto>)null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Patch_Collection_EmptyList_ShouldReturnEmptyList()
        {
            // Act
            var result = Mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(new List<PatchTestUpdateDto>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region DI Patch overloads

        [Fact]
        public void Patch_ViaISimpleMapper_NewObject_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var dto = new PatchTestUpdateDto
            {
                Name = "DI Patch",
                Email = null,
                Age = 50
            };

            // Act
            var entity = mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("DI Patch", entity.Name);
            Assert.Equal("original@test.com", entity.Email);  // null skipped → constructor default
            Assert.Equal(50, entity.Age);
        }

        [Fact]
        public void Patch_ViaISimpleMapper_TypeInferred_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var dto = new PatchTestUpdateDto
            {
                Name = "DI TypeInferred",
                Email = "di@test.com"
            };

            // Act
            var entity = mapper.Patch<PatchTestEntity>((object)dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("DI TypeInferred", entity.Name);
            Assert.Equal("di@test.com", entity.Email);
        }

        [Fact]
        public void Patch_ViaISimpleMapper_Collection_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var dtos = new List<PatchTestUpdateDto>
            {
                new PatchTestUpdateDto { Name = "A", Age = 1 },
                new PatchTestUpdateDto { Name = "B", Age = 2 }
            };

            // Act
            var entities = mapper.Patch<PatchTestUpdateDto, PatchTestEntity>(dtos);

            // Assert
            Assert.NotNull(entities);
            Assert.Equal(2, entities.Count);
            Assert.Equal("A", entities[0].Name);
            Assert.Equal("B", entities[1].Name);
        }

        #endregion
    }
}
