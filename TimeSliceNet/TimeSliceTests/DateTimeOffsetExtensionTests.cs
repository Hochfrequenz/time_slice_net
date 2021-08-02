using System;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="DateTimeOffsetExtensions" />
    /// </summary>
    /// <remarks>Although it's just an internal class</remarks>
    public class DateTimeOffsetExtensionTests
    {
        [Test]
        public void TestStrippingSubSecondsDateTimeOffset()
        {
            var dto = new DateTimeOffset(2020, 1, 1, 0, 0, 0, 123, TimeSpan.Zero);
            var actual = dto.StripSubSecond();
            Assert.AreEqual(0, actual.Millisecond);
            Assert.AreEqual("2020-01-01T00:00:00.0000000+00:00", actual.ToString("O"));
        }

        [Test]
        public void TestStrippingSubSecondsDateTimeOffsetNullable()
        {
            var dto = (DateTimeOffset?)new DateTimeOffset(2020, 1, 1, 0, 0, 0, 123, TimeSpan.Zero);
            var actual = dto.StripSubSecond();
            Assert.IsTrue(actual.HasValue);
            Assert.AreEqual(0, actual.Value.Millisecond);
            Assert.AreEqual("2020-01-01T00:00:00.0000000+00:00", actual.Value.ToString("O"));
        }

        [Test]
        public void TestStrippingSubSecondsDateTimeOffsetNullableNoValue()
        {
            var dto = (DateTimeOffset?)null;
            var actual = dto.StripSubSecond();
            Assert.IsFalse(actual.HasValue);
        }
    }
}