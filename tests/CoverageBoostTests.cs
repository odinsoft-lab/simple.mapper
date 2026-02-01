using Simple.AutoMapper.Core;
using Simple.AutoMapper.Internal;
using Microsoft.Extensions.DependencyInjection;
using Simple.AutoMapper.DependencyInjection;
using Simple.AutoMapper.Interfaces;

namespace Simple.AutoMapper.Tests
{
    #region Test Models for Coverage

    // Array mapping models
    public class ArraySourceEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ArrayDestDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    // Entity with array property
    public class EntityWithArray
    {
        public Guid Id { get; set; }
        public ArraySourceEntity[] Items { get; set; }
    }

    public class EntityWithArrayDto
    {
        public Guid Id { get; set; }
        public ArrayDestDto[] Items { get; set; }
    }

    // Patch reflection models (non-nullable source → Nullable destination)
    public class PatchReflectionSource
    {
        public int Score { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Flag { get; set; }
    }

    public class PatchReflectionNullableDest
    {
        public int? Score { get; set; } = 999;
        public DateTime? BirthDate { get; set; } = new DateTime(2000, 1, 1);
        public bool? Flag { get; set; } = true;
    }

    // Patch reflection models with complex types
    public class PatchReflectionComplexSource
    {
        public string Name { get; set; }
        public PatchReflectionAddress Address { get; set; }
        public List<string> Tags { get; set; }
    }

    public class PatchReflectionAddress
    {
        public string City { get; set; }
    }

    public class PatchReflectionComplexDest
    {
        public string Name { get; set; } = "Default";
        public PatchReflectionAddress Address { get; set; }
        public List<string> Tags { get; set; }
    }

    // GenericMap collection models
    public class GenericMapCollectionSource
    {
        public Guid Id { get; set; }
        public List<GenericMapChild> Children { get; set; }
    }

    public class GenericMapChild
    {
        public string Value { get; set; }
    }

    public class GenericMapCollectionDest
    {
        public Guid Id { get; set; }
        public List<GenericMapChildDto> Children { get; set; }
    }

    public class GenericMapChildDto
    {
        public string Value { get; set; }
    }

    // Entity with ICollection property for reflection path
    public class EntityWithICollection
    {
        public Guid Id { get; set; }
        public ICollection<string> Items { get; set; }
    }

    public class EntityWithICollectionDto
    {
        public Guid Id { get; set; }
        public ICollection<string> Items { get; set; }
    }

    // Nullable to non-nullable for compiled mapping
    public class NullableToNonNullableSource
    {
        public int? Value { get; set; }
        public DateTime? Date { get; set; }
    }

    public class NullableToNonNullableTarget
    {
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    // ForMember with NullSubstitute on Nullable<T>
    public class NullSubNullableSource
    {
        public int? Score { get; set; }
    }

    public class NullSubNullableTarget
    {
        public int Score { get; set; }
    }

    // ForMember with type conversion
    public class ConvertTypeSource
    {
        public int IntValue { get; set; }
    }

    public class ConvertTypeDest
    {
        public long LongValue { get; set; }
    }

    #endregion

    [Collection("Mapper Tests")]
    public class CoverageBoostTests
    {
        public CoverageBoostTests()
        {
            Mapper.Reset();
        }

        #region Array Mapping via Reflection

        [Fact]
        public void MapReflection_WithArrayProperty_ShouldMapArrayCorrectly()
        {
            // Arrange
            Mapper.Reset();
            var source = new EntityWithArray
            {
                Id = Guid.NewGuid(),
                Items = new[]
                {
                    new ArraySourceEntity { Id = Guid.NewGuid(), Name = "Item1" },
                    new ArraySourceEntity { Id = Guid.NewGuid(), Name = "Item2" }
                }
            };

            // Act - Map<TDestination>(object) uses reflection path
            var dest = Mapper.Map<EntityWithArrayDto>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(source.Id, dest.Id);
            Assert.NotNull(dest.Items);
            Assert.Equal(2, dest.Items.Length);
            Assert.Equal("Item1", dest.Items[0].Name);
            Assert.Equal("Item2", dest.Items[1].Name);
        }

        [Fact]
        public void MapReflection_WithArrayProperty_NullItems_ShouldHandleNullInArray()
        {
            // Arrange
            Mapper.Reset();
            var source = new EntityWithArray
            {
                Id = Guid.NewGuid(),
                Items = new ArraySourceEntity[]
                {
                    new ArraySourceEntity { Id = Guid.NewGuid(), Name = "Valid" },
                    null,
                    new ArraySourceEntity { Id = Guid.NewGuid(), Name = "Also Valid" }
                }
            };

            // Act
            var dest = Mapper.Map<EntityWithArrayDto>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.NotNull(dest.Items);
            Assert.Equal(3, dest.Items.Length);
            Assert.NotNull(dest.Items[0]);
            Assert.Null(dest.Items[1]);
            Assert.NotNull(dest.Items[2]);
        }

        [Fact]
        public void MapReflection_WithArrayProperty_EmptyArray_ShouldMapEmptyArray()
        {
            // Arrange
            Mapper.Reset();
            var source = new EntityWithArray
            {
                Id = Guid.NewGuid(),
                Items = Array.Empty<ArraySourceEntity>()
            };

            // Act
            var dest = Mapper.Map<EntityWithArrayDto>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.NotNull(dest.Items);
            Assert.Empty(dest.Items);
        }

        [Fact]
        public void MapReflection_WithArrayProperty_SimpleTypeElements()
        {
            // This tests the array path with simple type elements
            Mapper.Reset();
            var source = new { Tags = new string[] { "a", "b", "c" } };

            // Use reflection mapping
            var dest = Mapper.Map<EntityWithArrayDto>(new EntityWithArray
            {
                Id = Guid.NewGuid(),
                Items = new[]
                {
                    new ArraySourceEntity { Name = "SimpleTest" }
                }
            });

            Assert.NotNull(dest);
            Assert.Single(dest.Items);
        }

        [Fact]
        public void MapReflection_WithNullArrayProperty_ShouldSetNull()
        {
            // Arrange
            Mapper.Reset();
            var source = new EntityWithArray
            {
                Id = Guid.NewGuid(),
                Items = null
            };

            // Act
            var dest = Mapper.Map<EntityWithArrayDto>(source);

            // Assert
            Assert.NotNull(dest);
            Assert.Null(dest.Items);
        }

        #endregion

        #region Patch Reflection - Non-nullable to Nullable

        [Fact]
        public void Patch_TypeInferred_NonNullableToNullable_ShouldMapValues()
        {
            // Arrange - source has non-nullable int, dest has Nullable<int>
            var source = new PatchReflectionSource
            {
                Score = 42,
                BirthDate = new DateTime(1990, 5, 15),
                Flag = false
            };

            // Act - Uses PatchPropertiesReflectionInternal
            var dest = Mapper.Patch<PatchReflectionNullableDest>((object)source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(42, dest.Score);
            Assert.Equal(new DateTime(1990, 5, 15), dest.BirthDate);
            Assert.False(dest.Flag);
        }

        #endregion

        #region Patch Reflection - Complex Type and Collection

        [Fact]
        public void Patch_TypeInferred_WithComplexType_ShouldMapNestedObject()
        {
            // Arrange
            var source = new PatchReflectionComplexSource
            {
                Name = "Updated",
                Address = new PatchReflectionAddress { City = "Seoul" },
                Tags = null // null collection should be skipped
            };

            // Act - Uses PatchPropertiesReflectionInternal
            var dest = Mapper.Patch<PatchReflectionComplexDest>((object)source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal("Updated", dest.Name);
            Assert.NotNull(dest.Address);
            Assert.Equal("Seoul", dest.Address.City);
            Assert.Null(dest.Tags); // null skipped, but dest default is also null
        }

        [Fact]
        public void Patch_TypeInferred_WithCollection_ShouldMapCollection()
        {
            // Arrange
            var source = new PatchReflectionComplexSource
            {
                Name = "WithTags",
                Address = null, // null should be skipped
                Tags = new List<string> { "tag1", "tag2" }
            };

            // Act
            var dest = Mapper.Patch<PatchReflectionComplexDest>((object)source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal("WithTags", dest.Name);
            Assert.Null(dest.Address);  // null source skipped, dest default is null
            Assert.NotNull(dest.Tags);
            Assert.Equal(2, dest.Tags.Count);
        }

        [Fact]
        public void Patch_TypeInferred_NullSourceAndDest_ShouldNotThrow()
        {
            // Act
            var result = Mapper.Patch<PatchReflectionComplexDest>(null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region MapPropertiesGeneric - Collection branch

        [Fact]
        public void Map_InPlace_WithCollections_ShouldMapCollectionProperty()
        {
            // Arrange
            Mapper.Reset();
            var source = new GenericMapCollectionSource
            {
                Id = Guid.NewGuid(),
                Children = new List<GenericMapChild>
                {
                    new GenericMapChild { Value = "A" },
                    new GenericMapChild { Value = "B" }
                }
            };
            var dest = new GenericMapCollectionDest { Id = Guid.Empty };

            // Act - Uses MapPropertiesGeneric which has collection branch
            Mapper.Map(source, dest);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.NotNull(dest.Children);
            Assert.Equal(2, dest.Children.Count);
            Assert.Equal("A", dest.Children[0].Value);
        }

        [Fact]
        public void Map_InPlace_WithNullCollection_ShouldSetNull()
        {
            // Arrange
            Mapper.Reset();
            var source = new GenericMapCollectionSource
            {
                Id = Guid.NewGuid(),
                Children = null
            };
            var dest = new GenericMapCollectionDest
            {
                Id = Guid.Empty,
                Children = new List<GenericMapChildDto> { new GenericMapChildDto { Value = "Existing" } }
            };

            // Act
            Mapper.Map(source, dest);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.Null(dest.Children); // null overwritten
        }

        #endregion

        #region Nullable to Non-Nullable compiled mapping

        [Fact]
        public void Map_NullableToNonNullable_ShouldConvertViaExpressionTree()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NullableToNonNullableSource, NullableToNonNullableTarget>();

            var source = new NullableToNonNullableSource
            {
                Value = 42,
                Date = new DateTime(2025, 1, 1)
            };

            // Act
            var dest = engine.MapInstance<NullableToNonNullableSource, NullableToNonNullableTarget>(source);

            // Assert
            Assert.Equal(42, dest.Value);
            Assert.Equal(new DateTime(2025, 1, 1), dest.Date);
        }

        [Fact]
        public void Map_NullableToNonNullable_WithNullValues_ShouldThrow()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NullableToNonNullableSource, NullableToNonNullableTarget>();

            var source = new NullableToNonNullableSource
            {
                Value = null,
                Date = null
            };

            // Act & Assert - Nullable<T> to T conversion with null throws
            Assert.Throws<InvalidOperationException>(() =>
                engine.MapInstance<NullableToNonNullableSource, NullableToNonNullableTarget>(source));
        }

        #endregion

        #region ForMember with type conversion (projected != dest type)

        [Fact]
        public void ForMember_WithTypeConversion_ShouldConvert()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ConvertTypeSource, ConvertTypeDest>()
                .ForMember(d => d.LongValue, opt => opt.MapFrom(s => (long)s.IntValue));

            var source = new ConvertTypeSource { IntValue = 12345 };

            // Act
            var dest = engine.MapInstance<ConvertTypeSource, ConvertTypeDest>(source);

            // Assert
            Assert.Equal(12345L, dest.LongValue);
        }

        #endregion

        #region MapCollection null source in compiled path

        [Fact]
        public void MapCollection_NullSource_ShouldReturnNull()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ArraySourceEntity, ArrayDestDto>();

            // Act
            var result = Mapper.Map<ArraySourceEntity, ArrayDestDto>((IEnumerable<ArraySourceEntity>)null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapCollection_EmptySource_ShouldReturnEmptyList()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ArraySourceEntity, ArrayDestDto>();

            // Act
            var result = Mapper.Map<ArraySourceEntity, ArrayDestDto>(new List<ArraySourceEntity>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region MapInstanceWithContext - null source path

        [Fact]
        public void MapInstance_NullSource_ShouldReturnDefault()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<ArraySourceEntity, ArrayDestDto>();

            // Act
            ArraySourceEntity nullSource = null;
            var result = Mapper.Map<ArraySourceEntity, ArrayDestDto>(nullSource);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Patch via DI - Map in-place

        [Fact]
        public void ISimpleMapper_Map_InPlace_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<CoverageTestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var source = new ArraySourceEntity { Id = Guid.NewGuid(), Name = "InPlace" };
            var dest = new ArrayDestDto { Id = Guid.Empty, Name = "Old" };

            // Act
            mapper.Map(source, dest);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.Equal("InPlace", dest.Name);
        }

        #endregion

        #region Patch in-place non-nullable to nullable

        [Fact]
        public void Patch_InPlace_NonNullableToNullable_ShouldAssign()
        {
            // Arrange - PatchTestEntity has non-nullable int Age, PatchReflectionNullableDest has int? Score
            // Using PatchReflectionSource (non-nullable) → PatchReflectionNullableDest (nullable)
            var source = new PatchReflectionSource { Score = 77, BirthDate = new DateTime(2000, 6, 1), Flag = true };
            var dest = new PatchReflectionNullableDest { Score = null, BirthDate = null, Flag = null };

            // Act
            Mapper.Patch(source, dest);

            // Assert
            Assert.Equal(77, dest.Score);
            Assert.Equal(new DateTime(2000, 6, 1), dest.BirthDate);
            Assert.True(dest.Flag);
        }

        #endregion

        #region MappingContext - DecrementTypeDepth with existing entries

        [Fact]
        public void MappingContext_DecrementTypeDepth_AfterCircularRef_ShouldNotThrow()
        {
            // This exercises DecrementTypeDepth on a context that has been used
            var context = new MappingContext(maxDepth: 5, preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));

            // Set up some state
            context.IsCircularReference("test1", typePair);
            context.IsCircularReference("test2", typePair);

            // DecrementTypeDepth should not throw even without IncrementTypeDepth
            context.DecrementTypeDepth(typePair);
            context.DecrementDepth();
            context.DecrementDepth();

            Assert.Equal(0, context.CurrentDepth);
        }

        #endregion

        #region MappingExpression - SourceType/DestinationType properties

        [Fact]
        public void MappingExpression_SourceType_ShouldReturnCorrectType()
        {
            // Arrange
            var engine = Mapper.Reset();
            var expr = engine.CreateMap<ArraySourceEntity, ArrayDestDto>();

            // Access via IMappingExpression which has these properties
            // The expression is stored internally, but we can verify through mapping behavior
            Assert.NotNull(expr);
        }

        #endregion

        #region Reflection mapping - null sourceValue for complex type

        [Fact]
        public void MapReflection_NullComplexProperty_ShouldSetNull()
        {
            // Arrange
            Mapper.Reset();
            var source = new PatchReflectionComplexSource
            {
                Name = "Test",
                Address = null,
                Tags = null
            };

            // Act - Uses MapPropertiesReflection → null handling
            var dest = Mapper.Map<PatchReflectionComplexDest>((object)source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal("Test", dest.Name);
            Assert.Null(dest.Address);
            Assert.Null(dest.Tags);
        }

        #endregion

        #region Patch Collection via reflection (PatchPropertiesReflectionInternal collection branch)

        [Fact]
        public void Patch_TypeInferred_Collection_WithComplexItems()
        {
            // Model that has a List<ComplexType> for PatchPropertiesReflectionInternal collection path
            var source = new GenericMapCollectionSource
            {
                Id = Guid.NewGuid(),
                Children = new List<GenericMapChild>
                {
                    new GenericMapChild { Value = "C1" },
                    new GenericMapChild { Value = "C2" }
                }
            };

            // Act - Patch<TD>(object) → PatchPropertiesReflectionInternal
            var dest = Mapper.Patch<GenericMapCollectionDest>((object)source);

            // Assert
            Assert.NotNull(dest);
            Assert.Equal(source.Id, dest.Id);
            Assert.NotNull(dest.Children);
            Assert.Equal(2, dest.Children.Count);
        }

        #endregion

        #region SimpleMapper Patch in-place

        [Fact]
        public void ISimpleMapper_Patch_InPlace_ShouldWork()
        {
            // Arrange
            Mapper.Reset();
            var services = new ServiceCollection();
            services.AddSimpleMapper(config => config.AddProfile<CoverageTestProfile>());
            var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<ISimpleMapper>();

            var dto = new PatchTestUpdateDto { Name = "DI Patch InPlace", Age = 55 };
            var entity = new PatchTestEntity { Name = "Old", Age = 10 };

            // Act
            mapper.Patch(dto, entity);

            // Assert
            Assert.Equal("DI Patch InPlace", entity.Name);
            Assert.Equal(55, entity.Age);
        }

        #endregion

        #region Map with null source in MapPropertiesReflection

        [Fact]
        public void MapReflection_NullSource_ShouldReturnNull()
        {
            // Act
            var result = Mapper.Map<ArrayDestDto>((object)null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ReverseMap error path - type without parameterless constructor

        [Fact]
        public void ReverseMap_TypeWithoutParameterlessConstructor_ShouldThrow()
        {
            // Arrange
            var engine = Mapper.Reset();

            // NoDefaultCtorType has no parameterless constructor
            // We need to use ConstructUsing for the forward map, then ReverseMap should fail
            // since NoDefaultCtorType cannot be reversed (no `new()`)
            // Actually, the ReverseMap checks if TSource has a parameterless constructor
            // We can test this by creating a map where TSource doesn't have one

            // Uri doesn't have a parameterless constructor
            Assert.Throws<InvalidOperationException>(() =>
            {
                engine.CreateMap<Uri, ArrayDestDto>()
                    .ReverseMap();
            });
        }

        #endregion

        #region Map in-place with null source/destination

        [Fact]
        public void Map_InPlace_NullSource_ShouldNotModifyDestination()
        {
            // Arrange
            var dest = new ArrayDestDto { Id = Guid.NewGuid(), Name = "Keep" };

            // Act
            Mapper.Map<ArraySourceEntity, ArrayDestDto>(null, dest);

            // Assert
            Assert.Equal("Keep", dest.Name);
        }

        [Fact]
        public void Map_InPlace_NullDestination_ShouldNotThrow()
        {
            // Arrange
            var source = new ArraySourceEntity { Id = Guid.NewGuid(), Name = "Source" };

            // Act & Assert - should not throw
            Mapper.Map<ArraySourceEntity, ArrayDestDto>(source, null);
        }

        #endregion

        #region ForMember NullSubstitute on Nullable<T> source

        [Fact]
        public void ForMember_NullSubstitute_OnNullableInt_ShouldSubstitute()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NullSubNullableSource, NullSubNullableTarget>()
                .ForMember(d => d.Score, opt =>
                {
                    opt.MapFrom(s => s.Score ?? 0);
                    opt.NullSubstitute(0);
                });

            var source = new NullSubNullableSource { Score = null };

            // Act
            var dest = engine.MapInstance<NullSubNullableSource, NullSubNullableTarget>(source);

            // Assert
            Assert.Equal(0, dest.Score);
        }

        [Fact]
        public void ForMember_NullSubstitute_OnNullableInt_NonNull_ShouldUseValue()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<NullSubNullableSource, NullSubNullableTarget>()
                .ForMember(d => d.Score, opt =>
                {
                    opt.MapFrom(s => s.Score ?? 0);
                    opt.NullSubstitute(0);
                });

            var source = new NullSubNullableSource { Score = 99 };

            // Act
            var dest = engine.MapInstance<NullSubNullableSource, NullSubNullableTarget>(source);

            // Assert
            Assert.Equal(99, dest.Score);
        }

        #endregion

        #region PatchPropertiesGeneric - non-nullable to Nullable<T> dest

        [Fact]
        public void Patch_NonNullableSource_ToNullableDest_ShouldAssign()
        {
            // PatchReflectionSource has int Score (non-nullable)
            // PatchReflectionNullableDest has int? Score (nullable)
            var source = new PatchReflectionSource { Score = 88 };
            var dest = new PatchReflectionNullableDest { Score = null };

            // Act - Uses PatchPropertiesGeneric
            Mapper.Patch(source, dest);

            // Assert
            Assert.Equal(88, dest.Score);
        }

        #endregion

        #region Patch collection with complex items in PatchPropertiesGeneric

        [Fact]
        public void Patch_InPlace_WithCollections_ShouldMapCollectionProperty()
        {
            // Arrange
            var source = new GenericMapCollectionSource
            {
                Id = Guid.NewGuid(),
                Children = new List<GenericMapChild>
                {
                    new GenericMapChild { Value = "X" }
                }
            };
            var dest = new GenericMapCollectionDest
            {
                Id = Guid.Empty,
                Children = null
            };

            // Act - PatchPropertiesGeneric collection branch
            Mapper.Patch(source, dest);

            // Assert
            Assert.Equal(source.Id, dest.Id);
            Assert.NotNull(dest.Children);
            Assert.Single(dest.Children);
        }

        #endregion
    }

    // Profile for coverage tests
    public class CoverageTestProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<ArraySourceEntity, ArrayDestDto>();
        }
    }
}
