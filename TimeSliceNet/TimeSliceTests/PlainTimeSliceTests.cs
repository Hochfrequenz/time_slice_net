using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Tests <see cref="PlainTimeSlice"/>
    /// </summary>
    public class PlainTimeSliceTests
    {
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
        }
    }
}