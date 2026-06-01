using Simple.AutoMapper.Core;
using Xunit;

namespace Simple.AutoMapper.Tests
{
    /// <summary>
    /// DateOnly / TimeOnly support (v1.0.12).
    /// Before v1.0.12 these were silently skipped (left at default 0001-01-01 / 00:00:00)
    /// because, as structs, they matched neither IsSimpleType nor IsComplexType. They are now
    /// recognized as simple types and copied by value; nullable forms go through the Nullable&lt;&gt; path.
    /// </summary>
    [Collection("Mapper Tests")]
    public class DateOnlyTimeOnlyMappingTests
    {
        public class DateSrc
        {
            public DateOnly Date { get; set; }
            public DateOnly? NullableDate { get; set; }
            public TimeOnly Time { get; set; }
            public TimeOnly? NullableTime { get; set; }
        }

        public class DateDst
        {
            public DateOnly Date { get; set; }
            public DateOnly? NullableDate { get; set; }
            public TimeOnly Time { get; set; }
            public TimeOnly? NullableTime { get; set; }
        }

        [Fact]
        public void Map_DateOnly_And_TimeOnly_ShouldCopyByValue()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<DateSrc, DateDst>();

            var src = new DateSrc
            {
                Date = new DateOnly(2026, 4, 15),
                NullableDate = new DateOnly(2026, 5, 1),
                Time = new TimeOnly(9, 30),
                NullableTime = new TimeOnly(17, 0)
            };

            // Act
            var dst = engine.MapInstance<DateSrc, DateDst>(src);

            // Assert — values copied, not left at struct defaults
            Assert.NotNull(dst);
            Assert.Equal(new DateOnly(2026, 4, 15), dst.Date);
            Assert.Equal(new DateOnly(2026, 5, 1), dst.NullableDate);
            Assert.Equal(new TimeOnly(9, 30), dst.Time);
            Assert.Equal(new TimeOnly(17, 0), dst.NullableTime);
        }

        [Fact]
        public void Map_NullableDateOnly_TimeOnly_WhenNull_ShouldStayNull()
        {
            // Arrange
            var engine = Mapper.Reset();
            engine.CreateMap<DateSrc, DateDst>();

            // Act
            var dst = engine.MapInstance<DateSrc, DateDst>(new DateSrc { NullableDate = null, NullableTime = null });

            // Assert
            Assert.NotNull(dst);
            Assert.Null(dst.NullableDate);
            Assert.Null(dst.NullableTime);
        }
    }
}
