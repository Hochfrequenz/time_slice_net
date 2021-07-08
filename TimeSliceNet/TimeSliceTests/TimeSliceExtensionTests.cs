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
        /// Tests <see cref="TimeSliceExtensions.Overlaps"/>
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
            var start = DateTimeOffset.Parse(startString);
            DateTimeOffset? end = string.IsNullOrWhiteSpace(endString) ? null : DateTimeOffset.Parse(endString);
            var candidate = DateTimeOffset.Parse(candidateString);
            var pts = new PlainTimeSlice
            {
                Start = start,
                End = end
            };
            var actual = pts.Overlaps(candidate);
            Assert.AreEqual(expected, actual);
        }
    }
}