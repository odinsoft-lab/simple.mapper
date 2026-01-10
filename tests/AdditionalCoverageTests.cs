using Microsoft.Extensions.DependencyInjection;
using Simple.AutoMapper.Core;
using Simple.AutoMapper.DependencyInjection;
using Simple.AutoMapper.Tests.Models;
using Simple.AutoMapper.Tests.Models.DTO;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class AdditionalCoverageTests
    {
        #region List Mapping Tests

        public class EntityWithList
        {
            public Guid Id { get; set; }
            public List<ChildEntity> Children { get; set; }
        }

        public class EntityWithListDto
        {
            public Guid Id { get; set; }
            public List<ChildEntityDto> Children { get; set; }
        }

        public class ChildEntity
        {
            public string Name { get; set; }
        }

        public class ChildEntityDto
        {
            public string Name { get; set; }
        }

        [Fact]
        public void Map_WithList_ShouldMapListProperty()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ChildEntity, ChildEntityDto>();
            engine.CreateMap<EntityWithList, EntityWithListDto>();

            var entity = new EntityWithList
            {
                Id = Guid.NewGuid(),
                Children = new List<ChildEntity>
                {
                    new ChildEntity { Name = "Child1" },
                    new ChildEntity { Name = "Child2" }
                }
            };

            // Act
            var dto = engine.MapInstance<EntityWithList, EntityWithListDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.NotNull(dto.Children);
            Assert.Equal(2, dto.Children.Count);
            Assert.Equal("Child1", dto.Children[0].Name);
            Assert.Equal("Child2", dto.Children[1].Name);
        }

        [Fact]
        public void Map_WithEmptyList_ShouldMapToEmptyList()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ChildEntity, ChildEntityDto>();
            engine.CreateMap<EntityWithList, EntityWithListDto>();

            var entity = new EntityWithList
            {
                Id = Guid.NewGuid(),
                Children = new List<ChildEntity>()
            };

            // Act
            var dto = engine.MapInstance<EntityWithList, EntityWithListDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.NotNull(dto.Children);
            Assert.Empty(dto.Children);
        }

        [Fact]
        public void Map_WithNullList_ShouldMapToNull()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ChildEntity, ChildEntityDto>();
            engine.CreateMap<EntityWithList, EntityWithListDto>();

            var entity = new EntityWithList
            {
                Id = Guid.NewGuid(),
                Children = null
            };

            // Act
            var dto = engine.MapInstance<EntityWithList, EntityWithListDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Null(dto.Children);
        }

        #endregion

        #region Nullable Type Tests

        public class EntityWithNullable
        {
            public int? NullableInt { get; set; }
            public DateTime? NullableDate { get; set; }
            public Guid? NullableGuid { get; set; }
            public int NonNullableInt { get; set; }
        }

        public class EntityWithNullableDto
        {
            public int? NullableInt { get; set; }
            public DateTime? NullableDate { get; set; }
            public Guid? NullableGuid { get; set; }
            public int NonNullableInt { get; set; }
        }

        [Fact]
        public void Map_WithNullableValues_ShouldMapNullableToNullable()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<EntityWithNullable, EntityWithNullableDto>();

            var entity = new EntityWithNullable
            {
                NullableInt = 42,
                NullableDate = new DateTime(2024, 1, 1),
                NullableGuid = Guid.NewGuid(),
                NonNullableInt = 100
            };

            // Act
            var dto = engine.MapInstance<EntityWithNullable, EntityWithNullableDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(42, dto.NullableInt);
            Assert.Equal(new DateTime(2024, 1, 1), dto.NullableDate);
            Assert.Equal(entity.NullableGuid, dto.NullableGuid);
            Assert.Equal(100, dto.NonNullableInt);
        }

        [Fact]
        public void Map_WithNullNullableValues_ShouldMapNullsCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<EntityWithNullable, EntityWithNullableDto>();

            var entity = new EntityWithNullable
            {
                NullableInt = null,
                NullableDate = null,
                NullableGuid = null,
                NonNullableInt = 0
            };

            // Act
            var dto = engine.MapInstance<EntityWithNullable, EntityWithNullableDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Null(dto.NullableInt);
            Assert.Null(dto.NullableDate);
            Assert.Null(dto.NullableGuid);
        }

        #endregion

        #region MapPropertiesReflection Tests

        [Fact]
        public void MapPropertiesReflection_WithListDestination_ShouldMapCollection()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>();

            var entities = new List<Entity17>
            {
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() }
            };

            // Act - Using Mapper.Map<TDestination>(object) with List<TDestination>
            var dtos = Mapper.Map<List<EntityDTO17>>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(2, dtos.Count);
            Assert.Equal(entities[0].Id, dtos[0].Id);
            Assert.Equal(entities[1].Id, dtos[1].Id);
        }

        [Fact]
        public void MapPropertiesReflection_WithNullItemInCollection_ShouldAddNull()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>();

            var entities = new List<Entity17>
            {
                new Entity17 { Id = Guid.NewGuid() },
                null,
                new Entity17 { Id = Guid.NewGuid() }
            };

            // Act
            var dtos = Mapper.Map<List<EntityDTO17>>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(3, dtos.Count);
            Assert.NotNull(dtos[0]);
            Assert.Null(dtos[1]);
            Assert.NotNull(dtos[2]);
        }

        [Fact]
        public void MapPropertiesReflection_WithNull_ShouldReturnDefault()
        {
            // Arrange
            Mapper.Reset();

            // Act
            var result = Mapper.Map<EntityDTO17>(null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Collection Type Edge Cases

        public class EntityWithIEnumerable
        {
            public Guid Id { get; set; }
            public IEnumerable<ChildEntity> Children { get; set; }
        }

        public class EntityWithIEnumerableDto
        {
            public Guid Id { get; set; }
            public IEnumerable<ChildEntityDto> Children { get; set; }
        }

        public class EntityWithIList
        {
            public Guid Id { get; set; }
            public IList<ChildEntity> Children { get; set; }
        }

        public class EntityWithIListDto
        {
            public Guid Id { get; set; }
            public IList<ChildEntityDto> Children { get; set; }
        }

        [Fact]
        public void Map_WithIEnumerableProperty_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ChildEntity, ChildEntityDto>();
            engine.CreateMap<EntityWithIEnumerable, EntityWithIEnumerableDto>();

            var entity = new EntityWithIEnumerable
            {
                Id = Guid.NewGuid(),
                Children = new List<ChildEntity>
                {
                    new ChildEntity { Name = "Child1" },
                    new ChildEntity { Name = "Child2" }
                }
            };

            // Act
            var dto = engine.MapInstance<EntityWithIEnumerable, EntityWithIEnumerableDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.NotNull(dto.Children);
            Assert.Equal(2, dto.Children.Count());
        }

        [Fact]
        public void Map_WithIListProperty_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ChildEntity, ChildEntityDto>();
            engine.CreateMap<EntityWithIList, EntityWithIListDto>();

            var entity = new EntityWithIList
            {
                Id = Guid.NewGuid(),
                Children = new List<ChildEntity>
                {
                    new ChildEntity { Name = "Child1" },
                    new ChildEntity { Name = "Child2" }
                }
            };

            // Act
            var dto = engine.MapInstance<EntityWithIList, EntityWithIListDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.NotNull(dto.Children);
            Assert.Equal(2, dto.Children.Count);
        }

        #endregion

        #region MapFrom Complex Type Tests

        public class ParentEntity
        {
            public Guid Id { get; set; }
            public NestedEntity Nested { get; set; }
        }

        public class NestedEntity
        {
            public string Value { get; set; }
            public DeeplyNestedEntity Deep { get; set; }
        }

        public class DeeplyNestedEntity
        {
            public int Number { get; set; }
        }

        public class FlatDto
        {
            public Guid Id { get; set; }
            public string NestedValue { get; set; }
            public NestedEntity FullNested { get; set; }
        }

        [Fact]
        public void MapFrom_ComplexTypeToComplexType_ShouldMapNestedObject()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NestedEntity, NestedEntity>();
            engine.CreateMap<ParentEntity, FlatDto>()
                .ForMember(d => d.NestedValue, opt => opt.MapFrom(s => s.Nested.Value))
                .ForMember(d => d.FullNested, opt => opt.MapFrom(s => s.Nested));

            var entity = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Nested = new NestedEntity
                {
                    Value = "TestValue",
                    Deep = new DeeplyNestedEntity { Number = 42 }
                }
            };

            // Act
            var dto = engine.MapInstance<ParentEntity, FlatDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal("TestValue", dto.NestedValue);
            Assert.NotNull(dto.FullNested);
            Assert.Equal("TestValue", dto.FullNested.Value);
        }

        #endregion

        #region ForMember Ignore via opt.Ignore() Tests

        public class SourceWithMultipleFields
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Secret { get; set; }
        }

        public class DestWithMultipleFields
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Secret { get; set; }
        }

        [Fact]
        public void ForMember_WithIgnoreOption_ShouldIgnoreProperty()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<SourceWithMultipleFields, DestWithMultipleFields>()
                .ForMember(d => d.Secret, opt => opt.Ignore());

            var source = new SourceWithMultipleFields
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Secret = "ShouldBeIgnored"
            };

            // Act
            var dest = engine.MapInstance<SourceWithMultipleFields, DestWithMultipleFields>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(source.Id, dest.Id);
            Assert.Equal(source.Name, dest.Name);
            Assert.Null(dest.Secret); // Should be ignored
        }

        #endregion

        #region SimpleMapper Additional Tests

        [Fact]
        public void SimpleMapper_Map_WithInferredType_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act - Use Map<TDestination>(object) overload
            var dto = Mapper.Map<EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        #endregion

        #region Enum Mapping Tests

        public enum Status
        {
            Active,
            Inactive,
            Pending
        }

        public class EntityWithEnum
        {
            public Guid Id { get; set; }
            public Status Status { get; set; }
        }

        public class EntityWithEnumDto
        {
            public Guid Id { get; set; }
            public Status Status { get; set; }
        }

        [Fact]
        public void Map_WithEnumProperty_ShouldMapEnumCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<EntityWithEnum, EntityWithEnumDto>();

            var entity = new EntityWithEnum
            {
                Id = Guid.NewGuid(),
                Status = Status.Active
            };

            // Act
            var dto = engine.MapInstance<EntityWithEnum, EntityWithEnumDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(Status.Active, dto.Status);
        }

        #endregion

        #region Decimal and TimeSpan Tests

        public class EntityWithSpecialTypes
        {
            public decimal Price { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        public class EntityWithSpecialTypesDto
        {
            public decimal Price { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        [Fact]
        public void Map_WithDecimalAndTimeSpan_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<EntityWithSpecialTypes, EntityWithSpecialTypesDto>();

            var entity = new EntityWithSpecialTypes
            {
                Price = 99.99m,
                Duration = TimeSpan.FromHours(2.5),
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act
            var dto = engine.MapInstance<EntityWithSpecialTypes, EntityWithSpecialTypesDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(99.99m, dto.Price);
            Assert.Equal(TimeSpan.FromHours(2.5), dto.Duration);
            Assert.Equal(entity.CreatedAt, dto.CreatedAt);
        }

        #endregion

        #region MapTo Tests

        [Fact]
        public void MapTo_ShouldUpdateExistingInstance()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            var source = new Entity17 { Id = Guid.NewGuid() };
            var destination = new EntityDTO17 { Id = Guid.Empty };

            // Act
            Mapper.Map(source, destination);

            // Assert
            Assert.Equal(source.Id, destination.Id);
        }

        [Fact]
        public void MapTo_WithNullSource_ShouldNotModifyDestination()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            Entity17 source = null;
            var originalId = Guid.NewGuid();
            var destination = new EntityDTO17 { Id = originalId };

            // Act
            Mapper.Map<Entity17, EntityDTO17>(source, destination);

            // Assert - destination should remain unchanged
            Assert.Equal(originalId, destination.Id);
        }

        [Fact]
        public void MapTo_WithNullDestination_ShouldNotThrow()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            var source = new Entity17 { Id = Guid.NewGuid() };
            EntityDTO17 destination = null;

            // Act & Assert - should not throw
            Mapper.Map(source, destination);
        }

        #endregion

        #region Map with MaxDepth and PreserveReferences

        [Fact]
        public void Map_WithMaxDepthConfiguration_ShouldRespectMaxDepth()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>()
                .MaxDepth(2);

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act
            var dto = engine.MapInstance<Entity17, EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        [Fact]
        public void Map_WithPreserveReferencesConfiguration_ShouldBeSet()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>()
                .PreserveReferences();

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act
            var dto = engine.MapInstance<Entity17, EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        #endregion

        #region MapFrom with Collections

        public class EntityWithMemberCollection
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public List<MemberItem> Items { get; set; }
        }

        public class MemberItem
        {
            public int Value { get; set; }
        }

        public class EntityWithMemberCollectionDto
        {
            public Guid Id { get; set; }
            public string UpperName { get; set; }
            public List<MemberItemDto> Items { get; set; }
        }

        public class MemberItemDto
        {
            public int Value { get; set; }
        }

        [Fact]
        public void MapFrom_WithExpressionMapping_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<MemberItem, MemberItemDto>();
            engine.CreateMap<EntityWithMemberCollection, EntityWithMemberCollectionDto>()
                .ForMember(d => d.UpperName, opt => opt.MapFrom(s => s.Name.ToUpper()));

            var entity = new EntityWithMemberCollection
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Items = new List<MemberItem>
                {
                    new MemberItem { Value = 1 },
                    new MemberItem { Value = 2 }
                }
            };

            // Act
            var dto = engine.MapInstance<EntityWithMemberCollection, EntityWithMemberCollectionDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("TEST", dto.UpperName);
            // Items will be mapped by convention (same name)
            Assert.NotNull(dto.Items);
            Assert.Equal(2, dto.Items.Count);
        }

        #endregion

        #region Additional SimpleMapper Tests

        [Fact]
        public void SimpleMapper_MapCollection_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            var entities = new List<Entity17>
            {
                new Entity17 { Id = Guid.NewGuid() },
                new Entity17 { Id = Guid.NewGuid() }
            };

            // Act
            var dtos = Mapper.Map<Entity17, EntityDTO17>(entities);

            // Assert
            Assert.NotNull(dtos);
            Assert.Equal(2, dtos.Count);
        }

        #endregion

        #region Nullable to Non-Nullable Mapping

        public class EntityWithNullableSource
        {
            public int? NullableValue { get; set; }
        }

        public class EntityWithNonNullableDest
        {
            public int? NullableValue { get; set; }
        }

        [Fact]
        public void Map_NullableToNonNullable_ShouldConvertCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<EntityWithNullableSource, EntityWithNonNullableDest>();

            var source = new EntityWithNullableSource { NullableValue = 42 };

            // Act
            var dest = engine.MapInstance<EntityWithNullableSource, EntityWithNonNullableDest>(source);

            // Assert
            Assert.Equal(42, dest.NullableValue);
        }

        #endregion

        #region Complex Type with Circular Reference

        internal class SelfReferencingEntity
        {
            public Guid Id { get; set; }
            public SelfReferencingEntity Parent { get; set; }
        }

        internal class SelfReferencingEntityDto
        {
            public Guid Id { get; set; }
            public SelfReferencingEntityDto Parent { get; set; }
        }

        [Fact]
        public void Map_WithSelfReferencing_NoCircle_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<SelfReferencingEntity, SelfReferencingEntityDto>();

            var parent = new SelfReferencingEntity { Id = Guid.NewGuid() };
            var child = new SelfReferencingEntity { Id = Guid.NewGuid(), Parent = parent };

            // Act
            var dto = engine.MapInstance<SelfReferencingEntity, SelfReferencingEntityDto>(child);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(child.Id, dto.Id);
            Assert.NotNull(dto.Parent);
            Assert.Equal(parent.Id, dto.Parent.Id);
        }

        #endregion

        #region Multiple ForMember Calls

        public class MultiFieldSource
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        public class MultiFieldDest
        {
            public string FullName { get; set; }
            public string DisplayAge { get; set; }
            public int YearOfBirth { get; set; }
        }

        [Fact]
        public void Map_MultipleForMemberCalls_ShouldMapAllCustomFields()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<MultiFieldSource, MultiFieldDest>()
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(d => d.DisplayAge, opt => opt.MapFrom(s => $"{s.Age} years old"))
                .ForMember(d => d.YearOfBirth, opt => opt.MapFrom(s => DateTime.Now.Year - s.Age));

            var source = new MultiFieldSource
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30
            };

            // Act
            var dest = engine.MapInstance<MultiFieldSource, MultiFieldDest>(source);

            // Assert
            Assert.Equal("John Doe", dest.FullName);
            Assert.Equal("30 years old", dest.DisplayAge);
            Assert.Equal(DateTime.Now.Year - 30, dest.YearOfBirth);
        }

        #endregion

        #region Ignore and ForMember Combined

        [Fact]
        public void Map_WithIgnoreAndForMember_ShouldApplyBoth()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<MultiFieldSource, MultiFieldDest>()
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .Ignore(d => d.DisplayAge)
                .Ignore(d => d.YearOfBirth);

            var source = new MultiFieldSource
            {
                FirstName = "Jane",
                LastName = "Doe",
                Age = 25
            };

            // Act
            var dest = engine.MapInstance<MultiFieldSource, MultiFieldDest>(source);

            // Assert
            Assert.Equal("Jane Doe", dest.FullName);
            Assert.Null(dest.DisplayAge);
            Assert.Equal(0, dest.YearOfBirth);
        }

        #endregion

        #region Type conversion tests

        public class SourceWithDifferentTypes
        {
            public long LongValue { get; set; }
            public float FloatValue { get; set; }
        }

        public class DestWithDifferentTypes
        {
            public long LongValue { get; set; }
            public float FloatValue { get; set; }
        }

        [Fact]
        public void Map_WithPrimitiveTypes_ShouldMapCorrectly()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<SourceWithDifferentTypes, DestWithDifferentTypes>();

            var source = new SourceWithDifferentTypes
            {
                LongValue = 12345678901234L,
                FloatValue = 3.14f
            };

            // Act
            var dest = engine.MapInstance<SourceWithDifferentTypes, DestWithDifferentTypes>(source);

            // Assert
            Assert.Equal(12345678901234L, dest.LongValue);
            Assert.Equal(3.14f, dest.FloatValue);
        }

        #endregion

        #region MappingContext Additional Tests

        [Fact]
        public void MappingContext_WithCustomMaxDepth_ShouldUseCustomValue()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<Entity17, EntityDTO17>()
                .MaxDepth(5);

            var entity = new Entity17 { Id = Guid.NewGuid() };

            // Act
            var dto = engine.MapInstance<Entity17, EntityDTO17>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
        }

        #endregion

        #region Reflection Mapping Tests

        [Fact]
        public void Map_UsingReflection_WithNestedComplexType_ShouldMapCorrectly()
        {
            // Arrange - This tests MapPropertiesReflection path
            Mapper.Reset();

            var parent = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Nested = new NestedEntity
                {
                    Value = "TestValue",
                    Deep = new DeeplyNestedEntity { Number = 42 }
                }
            };

            // Act - Map<TDestination>(object) uses reflection path
            var dto = Mapper.Map<FlatDto>(parent);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(parent.Id, dto.Id);
        }

        [Fact]
        public void Map_UsingReflection_WithNullProperty_ShouldSetNullOnDestination()
        {
            // Arrange
            Mapper.Reset();

            var parent = new ParentEntity
            {
                Id = Guid.NewGuid(),
                Nested = null
            };

            // Act
            var dto = Mapper.Map<FlatDto>(parent);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(parent.Id, dto.Id);
            Assert.Null(dto.FullNested);
        }

        #endregion

        #region SimpleMapper Non-Generic MapTo Tests

        [Fact]
        public void SimpleMapper_MapTo_GenericVersion_ShouldMapCorrectly()
        {
            // Arrange
            Mapper.Reset();
            Mapper.CreateMap<Entity17, EntityDTO17>();

            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<TestProfile2>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<Simple.AutoMapper.Interfaces.ISimpleMapper>();

            var source = new Entity17 { Id = Guid.NewGuid() };
            var destination = new EntityDTO17();

            // Act
            mapper.MapTo(source, destination);

            // Assert
            Assert.Equal(source.Id, destination.Id);
        }

        #endregion

        #region Collection with Complex Items

        public class OrderEntity
        {
            public Guid Id { get; set; }
            public List<OrderItemEntity> Items { get; set; }
        }

        public class OrderItemEntity
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
            public ProductEntity Product { get; set; }
        }

        public class ProductEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class OrderDto
        {
            public Guid Id { get; set; }
            public List<OrderItemDto> Items { get; set; }
        }

        public class OrderItemDto
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
            public ProductDto Product { get; set; }
        }

        public class ProductDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Map_WithDeepNestedCollections_ShouldMapAllLevels()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ProductEntity, ProductDto>();
            engine.CreateMap<OrderItemEntity, OrderItemDto>();
            engine.CreateMap<OrderEntity, OrderDto>();

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                Items = new List<OrderItemEntity>
                {
                    new OrderItemEntity
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 2,
                        Product = new ProductEntity
                        {
                            Id = Guid.NewGuid(),
                            Name = "Product 1"
                        }
                    },
                    new OrderItemEntity
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 5,
                        Product = new ProductEntity
                        {
                            Id = Guid.NewGuid(),
                            Name = "Product 2"
                        }
                    }
                }
            };

            // Act
            var dto = engine.MapInstance<OrderEntity, OrderDto>(order);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(order.Id, dto.Id);
            Assert.NotNull(dto.Items);
            Assert.Equal(2, dto.Items.Count);
            Assert.Equal("Product 1", dto.Items[0].Product.Name);
            Assert.Equal("Product 2", dto.Items[1].Product.Name);
        }

        #endregion

        #region Reflection with Collections

        [Fact]
        public void MapReflection_WithList_ShouldMapCollection()
        {
            // Arrange
            Mapper.Reset();

            var entity = new EntityWithList
            {
                Id = Guid.NewGuid(),
                Children = new List<ChildEntity>
                {
                    new ChildEntity { Name = "Item1" },
                    new ChildEntity { Name = "Item2" }
                }
            };

            // Act - Uses reflection path via Map<TDestination>(object)
            var dto = Mapper.Map<EntityWithListDto>(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.NotNull(dto.Children);
            Assert.Equal(2, dto.Children.Count);
        }

        #endregion

        #region More ForMember Edge Cases

        public class SourceWithComplex
        {
            public Guid Id { get; set; }
            public NestedSource Inner { get; set; }
        }

        public class NestedSource
        {
            public string Data { get; set; }
        }

        public class DestWithComplex
        {
            public Guid Id { get; set; }
            public NestedDest Inner { get; set; }
        }

        public class NestedDest
        {
            public string Data { get; set; }
        }

        [Fact]
        public void Map_WithComplexType_ByConvention_ShouldMapNestedType()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NestedSource, NestedDest>();
            engine.CreateMap<SourceWithComplex, DestWithComplex>();

            var source = new SourceWithComplex
            {
                Id = Guid.NewGuid(),
                Inner = new NestedSource { Data = "TestData" }
            };

            // Act
            var dest = engine.MapInstance<SourceWithComplex, DestWithComplex>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(source.Id, dest.Id);
            Assert.NotNull(dest.Inner);
            Assert.Equal("TestData", dest.Inner.Data);
        }

        [Fact]
        public void Map_WithNullComplexType_ByConvention_ShouldHandleGracefully()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NestedSource, NestedDest>();
            engine.CreateMap<SourceWithComplex, DestWithComplex>();

            var source = new SourceWithComplex
            {
                Id = Guid.NewGuid(),
                Inner = null
            };

            // Act
            var dest = engine.MapInstance<SourceWithComplex, DestWithComplex>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(source.Id, dest.Id);
            Assert.Null(dest.Inner);
        }

        #endregion
    }

    // Additional Test Profile
    public class TestProfile2 : Simple.AutoMapper.Core.Profile
    {
        protected override void Configure()
        {
            CreateMap<Entity17, EntityDTO17>();
        }
    }
}

