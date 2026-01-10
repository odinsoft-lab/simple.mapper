using Simple.AutoMapper.Internal;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class TypePairTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties()
        {
            // Arrange & Act
            var pair = new TypePair(typeof(string), typeof(int));

            // Assert
            Assert.Equal(typeof(string), pair.SourceType);
            Assert.Equal(typeof(int), pair.DestinationType);
        }

        [Fact]
        public void Equals_SameTypes_ShouldReturnTrue()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            Assert.True(pair1.Equals(pair2));
            Assert.Equal(pair1, pair2);
        }

        [Fact]
        public void Equals_DifferentSourceType_ShouldReturnFalse()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(double), typeof(int));

            // Act & Assert
            Assert.False(pair1.Equals(pair2));
            Assert.NotEqual(pair1, pair2);
        }

        [Fact]
        public void Equals_DifferentDestinationType_ShouldReturnFalse()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(string), typeof(double));

            // Act & Assert
            Assert.False(pair1.Equals(pair2));
        }

        [Fact]
        public void Equals_WithObject_SameValues_ShouldReturnTrue()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            object pair2 = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            Assert.True(pair1.Equals(pair2));
        }

        [Fact]
        public void Equals_WithObject_DifferentType_ShouldReturnFalse()
        {
            // Arrange
            var pair = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            Assert.False(pair.Equals("not a pair"));
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var pair = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            Assert.False(pair.Equals(null));
        }

        [Fact]
        public void GetHashCode_SameTypes_ShouldReturnSameHash()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            Assert.Equal(pair1.GetHashCode(), pair2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentTypes_ShouldReturnDifferentHash()
        {
            // Arrange
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(double), typeof(float));

            // Act & Assert
            Assert.NotEqual(pair1.GetHashCode(), pair2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithNullTypes_ShouldNotThrow()
        {
            // Arrange
            var pair = new TypePair(null, null);

            // Act & Assert - should not throw
            var hash = pair.GetHashCode();
            Assert.Equal(0, hash);
        }

        [Fact]
        public void TypePair_AsHashKey_ShouldWorkCorrectly()
        {
            // Arrange
            var dict = new Dictionary<TypePair, string>();
            var pair1 = new TypePair(typeof(string), typeof(int));
            var pair2 = new TypePair(typeof(string), typeof(int));

            // Act
            dict[pair1] = "test";

            // Assert
            Assert.True(dict.ContainsKey(pair2));
            Assert.Equal("test", dict[pair2]);
        }
    }
}
