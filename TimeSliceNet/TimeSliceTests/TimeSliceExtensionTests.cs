using System;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Tests <see cref="TimeSliceExtensions"/>
    /// </summary>
    public class TimeSliceExtensionTests
    {
        /// <summary>
        /// Tests <see cref="TimeSliceExtensions.IsOpen"/>
        /// </summary>
        [Test]
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", false)] // has an end
        [TestCase("2021-07-01T00:00:00Z", null, true)] // has no end
        public void TestIsOpen(string startString, string endString, bool expected)
        {
            var pts = TestHelper.CreateTimeSlice(startString, endString);
            var actual = pts.IsOpen();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="TimeSliceExtensions.IsOpen"/>
        /// </summary>
        [Test]
        public void TestInfinityIsOpen()
        {
            var pts = new PlainTimeSlice
            {
                Start = DateTimeOffset.UtcNow,
                End = DateTimeOffset.MaxValue
            };
            Assert.IsTrue(pts.IsOpen());
        }

        /// <summary>
        /// Tests <see cref="TimeSliceExtensions.Overlaps(TimeSlice.ITimeSlice,System.DateTimeOffset)"/>
        /// </summary>
        [Test]
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-06-30T23:59:59Z", false)] // before
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-07-01T00:00:00Z", true)] // start date is inclusive
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-07-01T12:00:00Z", true)] // easy 1
        [TestCase("2021-07-01T00:00:00Z", null, "2021-07-01T12:00:00Z", true)] // easy with open end
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-07-01T23:59:59Z", true)] // easy 2
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-07-02T00:00:00Z", false)] // end date is exclusive
        [TestCase("2021-07-01T00:00:00Z", "2021-07-02T00:00:00Z", "2021-07-03T00:00:00Z", false)] // after
        public void TestOverlapsWithDateTime(string startString, string endString, string candidateString, bool expected)
        {
            var candidate = DateTimeOffset.Parse(candidateString);
            var pts = TestHelper.CreateTimeSlice(startString, endString);
            var actual = pts.Overlaps(candidate);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="TimeSliceExtensions.Overlaps(TimeSlice.ITimeSlice,TimeSlice.ITimeSlice)"/>
        /// </summary>
        [Test]
        [TestCase("2021-07-01T00:00:00Z", null, "2021-07-01T00:00:00Z", null, true)] // 2 open (identical) time slices
        [TestCase("2021-06-01T00:00:00Z", null, "2021-07-01T00:00:00Z", null, true)] // 2 open time slices
        [TestCase("2021-06-01T00:00:00Z", "2021-07-01T00:00:00Z", "2021-07-01T00:00:00Z", null, false)]
        [TestCase("2021-06-01T00:00:00Z", "2021-08-01T00:00:00Z", "2021-07-01T00:00:00Z", "2021-07-15T00:00:00Z", true)]
        [TestCase("2021-07-01T00:00:00Z", null, "2021-07-01T00:00:00Z", "2021-08-01T00:00:00Z", true)]
        public void TestOverlapsWithOtherTimeSlice(string startStringA, string endStringA, string startStringB, string endStringB, bool expected)
        {
            var ptsA = TestHelper.CreateTimeSlice(startStringA, endStringA);
            var ptsB = TestHelper.CreateTimeSlice(startStringB, endStringB);
            var actual = ptsA.Overlaps(ptsB);
            Assert.AreEqual(actual, ptsB.Overlaps(ptsA)); // if A overlaps B, then B also overlaps A
            Assert.AreEqual(expected, actual);
        }
    }
}