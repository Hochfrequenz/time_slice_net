using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Tests <see cref="PlainTimeSlice"/>
    /// </summary>
    public class PlainTimeSliceTests
    {
        private readonly JsonSerializerOptions _minifyOptions = new()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Tests that the duration of plain time slices is calculated as expected.
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
        /// Tests <see cref="PlainTimeSlice.Equals(object?)"/>
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
            {
                Assert.AreEqual(ptsA.GetHashCode(), ptsB.GetHashCode());
            }
            else
            {
                Assert.AreNotEqual(ptsA.GetHashCode(), ptsB.GetHashCode());
            }
            Assert.IsFalse(ptsA.Validate(null).Any()); // check validity
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
            // this comparison is supposed to be suspecious ;)
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(pts.Equals(new StringBuilder()), "Objects that do not implement the time slice interface should not be considered equal.");
        }

        /// <summary>
        /// Tests deserialization of <see cref="PlainTimeSlice"/> and that any datetimeoffset is returned without offset
        /// </summary>
        [Test]
        [TestCase("{\"start\":\"2021-07-01T00:00:00Z\",\"end\":\"2021-08-01T00:00:00Z\"}")]
        [TestCase("{\"start\":\"2021-07-01T00:00:00+00:00\",\"end\":\"2021-08-01T00:00:00+00:00\"}")]
        [TestCase("{\"start\":\"2021-07-01T02:00:00+02:00\",\"end\":\"2021-07-31T22:00:00-02:00\"}")]
        public void TestDeserializationWithoutOffset(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var expected = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 0, 0, 0, TimeSpan.Zero)
            };
            Assert.AreEqual(actual: pts, expected: expected);
            Assert.IsFalse(pts.Validate(null).Any()); // check validity
        }

        /// <summary>
        /// Tests deserialization of <see cref="PlainTimeSlice"/> "wrong"/invalid time slices
        /// </summary>
        [Test]
        [TestCase("{\"start\":\"2021-07-01T00:00:00Z\",\"end\":\"2021-07-31T23:59:59+02:00\"}")] // 23:59:59
        [TestCase("{\"start\":\"2021-07-01T00:00:00+00:00\",\"end\":\"2021-07-31T21:59:59Z\"}")] // similar to 23:59:59 but as UTC
        [TestCase("{\"start\":\"2021-07-01T00:00:00+00:00\",\"end\":\"2020-07-01T00:00:00Z\"}")] // similar to 23:59:59 but as UTC
        public void TestInvalidTimeSlices(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            Assert.IsNotNull(pts);
            Assert.IsTrue(pts.Validate(null).Any());
        }

        /// <summary>
        /// Tests round trip (de-)serialization of <see cref="PlainTimeSlice"/>
        /// </summary>
        [Test]
        [TestCase("{\"start\":\"2021-07-01T00:00:00+00:00\",\"end\":\"2021-08-01T00:00:00+00:00\"}")]
        [TestCase("{\"start\":\"2021-07-01T00:00:00+00:00\",\"end\":null}")]
        public void TestDeserializationRoundTrip(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var reserializedPts = JsonSerializer.Serialize(pts, _minifyOptions);
            Assert.AreEqual(json, reserializedPts);
            Assert.IsFalse(pts.Validate(null).Any());
        }

        /// <summary>
        /// Tests (de-)serialization of null as end date works
        /// </summary>
        [Test]
        [TestCase("{\"start\":\"2021-07-01T00:00:00Z\",\"end\":null}")]
        [TestCase("{\"start\":\"2021-07-01T00:00:00Z\"}")]
        public void TestNullEndDateDeserialization(string json)
        {
            var pts = JsonSerializer.Deserialize<PlainTimeSlice>(json);
            var expected = new PlainTimeSlice
            {
                Start = new DateTimeOffset(2021, 7, 1, 0, 0, 0, TimeSpan.Zero),
                End = null
            };
            Assert.AreEqual(actual: pts, expected: expected);
            Assert.IsFalse(pts.Validate(null).Any());
        }

        /// <summary>
        /// Tests that the start date must not be null.
        /// </summary>
        [Test]
        [TestCase("{\"start\":null,\"end\":\"2021-08-01T00:00:00\"}")]
        [TestCase("{\"start\":null\"}")]
        public void TestNullStartDateDeserialization(string json)
        {
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<PlainTimeSlice>(json));
        }

        /// <summary>
        /// Tests that the (de)-serialization of <see cref="PlainTimeSlice"/> without a offset or time zone information like "Z" fails with a format exception.
        /// </summary>
        /// <remarks>
        /// If there's a bug, this test might fail on a local computer that "lives" in a culture with a local time with a != ZERO offset from UTC.
        /// ToDo: Find a way to mock the DateTime Provider such that we can replicate this behaviour also in systems that live in UTC (like the github actions running the test).
        /// </remarks>
        [Test]
        public void TestDeserializationEnforceOffset()
        {
            const string json = "{\"start\":\"2021-07-01T00:00:00\",\"end\":\"2021-08-01T00:00:00\"}";
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<PlainTimeSlice>(json));
        }
    }
}