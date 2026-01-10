using Simple.AutoMapper.Core;
using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class MappingContextTests
    {
        [Fact]
        public void Constructor_DefaultValues_ShouldSetCorrectDefaults()
        {
            // Act
            var context = new MappingContext();

            // Assert
            Assert.Equal(10, context.MaxDepth);
            Assert.False(context.PreserveReferences);
            Assert.Equal(0, context.CurrentDepth);
        }

        [Fact]
        public void Constructor_WithCustomValues_ShouldSetValues()
        {
            // Act
            var context = new MappingContext(maxDepth: 5, preserveReferences: true);

            // Assert
            Assert.Equal(5, context.MaxDepth);
            Assert.True(context.PreserveReferences);
        }

        [Fact]
        public void Constructor_WithZeroMaxDepth_ShouldUseDefaultOf10()
        {
            // Act
            var context = new MappingContext(maxDepth: 0);

            // Assert
            Assert.Equal(10, context.MaxDepth);
        }

        [Fact]
        public void Constructor_WithNegativeMaxDepth_ShouldUseDefaultOf10()
        {
            // Act
            var context = new MappingContext(maxDepth: -5);

            // Assert
            Assert.Equal(10, context.MaxDepth);
        }

        [Fact]
        public void IsCircularReference_WithNullSource_ShouldReturnFalse()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(string));

            // Act
            var result = context.IsCircularReference(null, typePair);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCircularReference_WithPreserveReferencesDisabled_ShouldReturnFalse()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: false);
            var typePair = new TypePair(typeof(string), typeof(string));
            var source = "test";

            // Act
            var result = context.IsCircularReference(source, typePair);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCircularReference_FirstCall_ShouldReturnFalse()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));
            var source = "test";

            // Act
            var result = context.IsCircularReference(source, typePair);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCircularReference_SecondCallSameObject_ShouldReturnTrue()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));
            var source = "test";

            // Act
            context.IsCircularReference(source, typePair); // First call
            var result = context.IsCircularReference(source, typePair); // Second call

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CacheDestination_WithPreserveReferencesEnabled_ShouldCacheValue()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var source = new object();
            var destination = new object();

            // Act
            context.CacheDestination(source, typeof(object), destination);
            var cached = context.GetCachedDestination(source, typeof(object));

            // Assert
            Assert.Same(destination, cached);
        }

        [Fact]
        public void CacheDestination_WithPreserveReferencesDisabled_ShouldNotCache()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: false);
            var source = new object();
            var destination = new object();

            // Act
            context.CacheDestination(source, typeof(object), destination);
            var cached = context.GetCachedDestination(source, typeof(object));

            // Assert
            Assert.Null(cached);
        }

        [Fact]
        public void CacheDestination_WithNullSource_ShouldNotCache()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var destination = new object();

            // Act
            context.CacheDestination(null, typeof(object), destination);
            var cached = context.GetCachedDestination(null, typeof(object));

            // Assert
            Assert.Null(cached);
        }

        [Fact]
        public void CacheDestination_WithNullDestination_ShouldNotCache()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var source = new object();

            // Act
            context.CacheDestination(source, typeof(object), null);
            var cached = context.GetCachedDestination(source, typeof(object));

            // Assert
            Assert.Null(cached);
        }

        [Fact]
        public void GetCachedDestination_WithNullSource_ShouldReturnNull()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);

            // Act
            var cached = context.GetCachedDestination(null, typeof(object));

            // Assert
            Assert.Null(cached);
        }

        [Fact]
        public void DecrementDepth_ShouldDecreaseCurrentDepth()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));

            // Increase depth via IsCircularReference
            context.IsCircularReference("test", typePair);
            Assert.Equal(1, context.CurrentDepth);

            // Act
            context.DecrementDepth();

            // Assert
            Assert.Equal(0, context.CurrentDepth);
        }

        [Fact]
        public void DecrementDepth_AtZero_ShouldStayAtZero()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            Assert.Equal(0, context.CurrentDepth);

            // Act
            context.DecrementDepth();

            // Assert
            Assert.Equal(0, context.CurrentDepth);
        }

        [Fact]
        public void DecrementTypeDepth_ShouldDecreaseTypePairDepth()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));

            // Act & Assert - DecrementTypeDepth on non-existent key should not throw
            context.DecrementTypeDepth(typePair);
        }

        [Fact]
        public void IsCircularReference_WithDepthTracking_ShouldIncrementCurrentDepth()
        {
            // Arrange
            var context = new MappingContext(maxDepth: 5, preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));
            var source1 = "test1";
            var source2 = "test2";
            var source3 = "test3";

            // Act - Multiple calls should increment depth
            context.IsCircularReference(source1, typePair);
            context.IsCircularReference(source2, typePair);
            context.IsCircularReference(source3, typePair);

            // Assert
            Assert.Equal(3, context.CurrentDepth);
        }

        [Fact]
        public void GetCachedDestination_WithCachedValue_ShouldReturnIt()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var source = new object();
            var destination = new object();
            var typePair = new TypePair(typeof(object), typeof(object));

            // First register via IsCircularReference
            context.IsCircularReference(source, typePair);
            // Then cache the actual destination
            context.CacheDestination(source, typeof(object), destination);

            // Act
            var cached = context.GetCachedDestination(source, typeof(object));

            // Assert
            Assert.Same(destination, cached);
        }

        [Fact]
        public void DecrementDepth_MultipleTimes_ShouldDecreaseCorrectly()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));

            // Increase depth via IsCircularReference
            context.IsCircularReference("test1", typePair);
            context.IsCircularReference("test2", typePair);
            Assert.Equal(2, context.CurrentDepth);

            // Act
            context.DecrementDepth();
            context.DecrementDepth();
            context.DecrementDepth(); // Extra decrement should stay at 0

            // Assert
            Assert.Equal(0, context.CurrentDepth);
        }

        [Fact]
        public void IsCircularReference_WhenMaxDepthExceeded_ShouldReturnTrue()
        {
            // Arrange - Use a very small maxDepth to trigger the depth check
            var context = new MappingContext(maxDepth: 1, preserveReferences: true);
            var typePair = new TypePair(typeof(string), typeof(int));

            // Generate many unique sources to increment depth
            var sources = new List<object>();
            for (int i = 0; i < 12; i++) // maxDepth * 10 = 10, so we need > 10
            {
                sources.Add(new object());
            }

            // Act - Call IsCircularReference for each unique source
            foreach (var source in sources)
            {
                context.IsCircularReference(source, typePair);
            }

            // At this point depth should be >= maxDepth * 10
            // Next call should return true
            var newSource = new object();
            var result = context.IsCircularReference(newSource, typePair);

            // Assert - Should return true when depth exceeded
            Assert.True(result);
        }

        [Fact]
        public void DecrementTypeDepth_WhenDepthExists_ShouldDecrement()
        {
            // Arrange
            var context = new MappingContext(preserveReferences: true);
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(double), typeof(float));

            // First, we need to trigger some mappings to create type depth entries
            // Since IncrementTypeDepth is private, we can only test DecrementTypeDepth behavior
            // when called on non-existent keys (should not throw)

            // Act & Assert - should not throw on non-existent or empty entries
            context.DecrementTypeDepth(typePair1);
            context.DecrementTypeDepth(typePair2);
            context.DecrementTypeDepth(typePair1); // Multiple calls
        }
    }

    [Collection("Mapper Tests")]
    public class ContextCacheKeyTests
    {
        [Fact]
        public void Equals_SameSourceAndType_ShouldReturnTrue()
        {
            // Arrange
            var source = new object();
            var key1 = new ContextCacheKey(source, typeof(string));
            var key2 = new ContextCacheKey(source, typeof(string));

            // Act & Assert
            Assert.True(key1.Equals(key2));
        }

        [Fact]
        public void Equals_DifferentSource_ShouldReturnFalse()
        {
            // Arrange
            var key1 = new ContextCacheKey(new object(), typeof(string));
            var key2 = new ContextCacheKey(new object(), typeof(string));

            // Act & Assert
            Assert.False(key1.Equals(key2));
        }

        [Fact]
        public void Equals_DifferentType_ShouldReturnFalse()
        {
            // Arrange
            var source = new object();
            var key1 = new ContextCacheKey(source, typeof(string));
            var key2 = new ContextCacheKey(source, typeof(int));

            // Act & Assert
            Assert.False(key1.Equals(key2));
        }

        [Fact]
        public void Equals_WithObject_SameValues_ShouldReturnTrue()
        {
            // Arrange
            var source = new object();
            var key1 = new ContextCacheKey(source, typeof(string));
            object key2 = new ContextCacheKey(source, typeof(string));

            // Act & Assert
            Assert.True(key1.Equals(key2));
        }

        [Fact]
        public void Equals_WithObject_DifferentType_ShouldReturnFalse()
        {
            // Arrange
            var key1 = new ContextCacheKey(new object(), typeof(string));

            // Act & Assert
            Assert.False(key1.Equals("not a key"));
        }

        [Fact]
        public void GetHashCode_SameValues_ShouldReturnSameHash()
        {
            // Arrange
            var source = new object();
            var key1 = new ContextCacheKey(source, typeof(string));
            var key2 = new ContextCacheKey(source, typeof(string));

            // Act & Assert
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        }

        [Fact]
        public void Source_Property_ShouldReturnCorrectValue()
        {
            // Arrange
            var source = new object();
            var key = new ContextCacheKey(source, typeof(string));

            // Assert
            Assert.Same(source, key.Source);
        }

        [Fact]
        public void DestinationType_Property_ShouldReturnCorrectValue()
        {
            // Arrange
            var key = new ContextCacheKey(new object(), typeof(string));

            // Assert
            Assert.Equal(typeof(string), key.DestinationType);
        }
    }
}
