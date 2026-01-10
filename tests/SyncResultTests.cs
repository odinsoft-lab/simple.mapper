using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests
{
    [Collection("Mapper Tests")]
    public class SyncResultTests
    {
        [Fact]
        public void Constructor_ShouldSetAllProperties()
        {
            // Arrange & Act
            var result = new SyncResult(10, 20, 30);

            // Assert
            Assert.Equal(10, result.Added);
            Assert.Equal(20, result.Updated);
            Assert.Equal(30, result.Removed);
        }

        [Fact]
        public void Constructor_WithZeroValues_ShouldWork()
        {
            // Arrange & Act
            var result = new SyncResult(0, 0, 0);

            // Assert
            Assert.Equal(0, result.Added);
            Assert.Equal(0, result.Updated);
            Assert.Equal(0, result.Removed);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var result = new SyncResult(5, 10, 3);

            // Act
            var str = result.ToString();

            // Assert
            Assert.Equal("Added=5, Updated=10, Removed=3", str);
        }

        [Fact]
        public void ToString_WithZeroValues_ShouldReturnFormattedString()
        {
            // Arrange
            var result = new SyncResult(0, 0, 0);

            // Act
            var str = result.ToString();

            // Assert
            Assert.Equal("Added=0, Updated=0, Removed=0", str);
        }

        [Fact]
        public void SyncResult_ShouldBeValueType()
        {
            // Arrange & Act
            var result = new SyncResult(1, 2, 3);

            // Assert
            Assert.True(result.GetType().IsValueType);
        }

        [Fact]
        public void SyncResult_DefaultValue_ShouldHaveZeroValues()
        {
            // Arrange & Act
            SyncResult result = default;

            // Assert
            Assert.Equal(0, result.Added);
            Assert.Equal(0, result.Updated);
            Assert.Equal(0, result.Removed);
        }

        [Fact]
        public void SyncResult_Equality_SameValues_ShouldBeEqual()
        {
            // Arrange
            var result1 = new SyncResult(1, 2, 3);
            var result2 = new SyncResult(1, 2, 3);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void SyncResult_Equality_DifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var result1 = new SyncResult(1, 2, 3);
            var result2 = new SyncResult(4, 5, 6);

            // Assert
            Assert.NotEqual(result1, result2);
        }
    }
}
