using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Tests <see cref="PlainTimeSlice" />
    /// </summary>
    public class PlainTimeSliceTests
    {
        private readonly JsonSerializerOptions _minifyOptions = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        ///     Tests that the duration of plain time slices is calculated as expected.
        /// </summary>
        /// <param name="startString"></param>
        /// <param name="endString"></param>
        /// <param name="expectedDurationSeconds"></param>
        [Test]
        [TestCase("2021-07-01T00:00:00Z", " ", null)]
        [TestCase("2021-07-01T00:00:00Z", "2021-07-01T00:01:00Z", 60)]
        public void TestDuration(string startString, string endString, int? expectedDurationSeconds)
        {
            var pts = TestHelper.CreateTimeSlice(startString, endString);
            var actualDuration = pts.Duration?.TotalSeconds;
            Assert.AreEqual(expectedDurationSeconds, actualDuration);
        }

        [Test]
        [TestCase("2021-07-01T00:00:00Z", " ", "Open time slice [2021-07-01T00:00:00.0000000+00:00 to infinity")]
        [TestCase("2021-07-01T00:00:00Z", "2021-07-01T00:01:00Z", "Time slice [2021-07-01T00:00:00.0000000+00:00 - 2021-07-01T00:01:00.0000000+00:00)")]
        public void TestToString(string startString, string endString, string expectedStringRepresentation)
        {
            var pts = TestHelper.CreateTimeSlice(startString, endString);
            var actualString = pts.ToString();
            Assert.AreEqual(expectedStringRepresentation, actualString);
        }

        /// <summary>
        ///     Tests <see cref="PlainTimeSlice.Equals(object?)" />
        /// </summary>
        [Test]
        [TestCase("2021-07-01T00:00:00Z", null, "2021-07-01T00:00:00Z", null, true)]
        [TestCase("2021-06-01T00:00:00Z", null, "2021-07-01T00:00:00Z", null, false)]
        [TestCase("2021-06-01T00:00:00Z", "2021-07-01T00:00:00Z", "2021-07-01T00:00:00Z", null, false)]
        [TestCase("2021-06-01T00:00:00Z", "2021-08-01T00:00:00Z", "2021-07-01T00:00:00Z", "2021-07-15T00:00:00Z", false)]
        [TestCase("2021-07-01T00:00:00Z", null, "2021-07-01T00:00:00Z", "2021-08-01T00:00:00Z", false)]
        public void TestEquals(string startStringA, string endStringA, string startStringB, string endStringB, bool expected)
        {
            var ptsA = TestHelper.CreateTimeSlice(startStringA, endStringA);
            var ptsB = TestHelper.CreateTimeSlice(startStringB, endStringB);
            var actual = ptsA.Equals(ptsB);
            Assert.AreEqual(actual, ptsB.Equals(ptsA)); // if A equals B, then B also equals A
            Assert.AreEqual(expected, actual);
            if (expected)
                Assert.AreEqual(ptsA.GetHashCode(), ptsB.GetHashCode());
            else
                Assert.AreNotEqual(ptsA.GetHashCode(), ptsB.GetHashCode());
            Assert.IsTrue(ptsA.IsValid());
        }

        [Test]
        public void TestObviouslyFalseEquals()
        {
            var pts = new PlainTimeSlice
            {
                Start = DateTimeOffset.UtcNow,
                End = DateTimeOffset.UtcNow + TimeSpan.FromDays(1)
            };
            Assert.IsFalse(pts.Equals(null), "Null must not equal any time slice.");
            // this comparison is supposed to be suspicious ;)
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(pts.Equals(new StringBuilder()), "Objects that do not implement the time slice interface should not be considered equal.");
        }

        /// <summary>
        ///     Tests deserialization of <see cref="PlainTimeSlice" /> and that any DateTimeOffset is returned without offset
        /// </summary>
        [Test]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00Z\",\"End\":\"2021-08-01T00:00:00Z\"}")]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00+00:00\",\"End\":\"2021-08-01T00:00:00+00:00\"}")]
        [TestCase("{\"Start\":\"2021-07-01T02:00:00+02:00\",\"End\":\"2021-07-31T22:00:00-02:00\"}")]
        public void TestDeserializationWithoutOffset(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var expected = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 0, 0, 0, TimeSpan.Zero)
            };
            Assert.AreEqual(actual: pts, expected: expected);
            Assert.IsNotNull(pts);
            Assert.IsTrue(pts.IsValid());
        }

        /// <summary>
        ///     Tests deserialization of <see cref="PlainTimeSlice" /> "wrong"/invalid time slices
        /// </summary>
        [Test]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00Z\",\"End\":\"2021-07-31T23:59:59+02:00\"}")] // 23:59:59
        [TestCase("{\"Start\":\"2021-07-01T00:00:00+00:00\",\"End\":\"2021-07-31T21:59:59Z\"}")] // similar to 23:59:59 but as UTC
        [TestCase("{\"Start\":\"2021-07-01T00:00:00+00:00\",\"End\":\"2020-07-01T00:00:00Z\"}")] // similar to 23:59:59 but as UTC
        public void TestInvalidTimeSlices(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            Assert.IsNotNull(pts);
            Assert.IsFalse(pts.IsValid());
        }

        /// <summary>
        ///     While milliseconds are interesting for high temporal resolution technical applications, they mostly annoy us when
        ///     storing information in a database (who knows the resolution there?) or (de)serializing between different systems.
        ///     In normal business applications we don't care for sub-second resolution.
        /// </summary>
        [Test]
        public void TestMillisecondsAreIgnored()
        {
            var ptsWithMilliseconds = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 8, 1, 0, 0, 0, 123, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 0, 0, 0, 456, TimeSpan.Zero)
            };
            var ptsWithoutMilliseconds = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 8, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 0, 0, 0, TimeSpan.Zero)
            };
            Assert.AreEqual(ptsWithMilliseconds, ptsWithoutMilliseconds);
        }

        /// <summary>
        ///     Tests round trip (de-)serialization of <see cref="PlainTimeSlice" />
        /// </summary>
        [Test]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00+00:00\",\"End\":\"2021-08-01T00:00:00+00:00\"}")]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00+00:00\",\"End\":null}")]
        public void TestDeserializationRoundTrip(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var reserializedPts = JsonSerializer.Serialize(pts, _minifyOptions);
            Assert.AreEqual(json, reserializedPts);
            Assert.IsNotNull(pts);
            Assert.IsTrue(pts.IsValid());
        }

        /// <summary>
        ///     Tests (de-)serialization of null as end date works
        /// </summary>
        [Test]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00Z\",\"End\":null}")]
        [TestCase("{\"Start\":\"2021-07-01T00:00:00Z\"}")]
        public void TestNullEndDateDeserialization(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var expected = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            Assert.AreEqual(actual: pts, expected: expected);
            Assert.IsNotNull(pts);
            Assert.IsTrue(pts.IsValid());
        }

        /// <summary>
        ///     Tests that the start date must not be null.
        /// </summary>
        [Test]
        [TestCase("{\"Start\":null,\"End\":\"2021-08-01T00:00:00\"}")]
        [TestCase("{\"Start\":null\"}")]
        public void TestNullStartDateDeserialization(string json)
        {
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<PlainTimeSlice>(json));
        }

        /// <summary>
        ///     Tests that the (de)-serialization of <see cref="PlainTimeSlice" /> without a offset or time zone information like "Z" fails with a format exception.
        /// </summary>
        /// <remarks>
        ///     If there's a bug, this test might fail on a local computer that "lives" in a culture with a local time with a != ZERO offset from UTC.
        ///     ToDo: Find a way to mock the DateTime Provider such that we can replicate this behaviour also in systems that live in UTC (like the github actions running the test).
        /// </remarks>
        [Test]
        public void TestDeserializationEnforceOffset()
        {
            const string json = "{\"Start\":\"2021-07-01T00:00:00\",\"End\":\"2021-08-01T00:00:00\"}";
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<PlainTimeSlice>(json));
        }

        [Test]
        public void TestNullIsNotEqualToTimeSlice()
        {
            var pts = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            Assert.AreNotEqual(pts, null);
            Assert.IsFalse(pts.Equals(null));
        }

        [Test]
        public void TestNoPlainTimeSliceIsNotEqualToTimeSlice()
        {
            var pts = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            Assert.AreNotEqual(pts, "asdasd");
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(pts.Equals("asdasd"));
        }

        [Test]
        public void TestSameTimeSliceHasSameHashCode()
        {
            var ptsA = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            var ptsB = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            Assert.AreEqual(ptsA, ptsB);
            var set = new HashSet<PlainTimeSlice>
            {
                ptsA, // adding a equal time slice
                ptsB // twice
            };
            Assert.AreEqual(1, set.Count); // but we'll keep only one
        }
    }
}