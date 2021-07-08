using System;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    public class PlainTimeSliceTests
    {
        [Test]
        [TestCase("2021-07-01T00:00:00Z", "2021-07-01T00:01:00Z", 60)]
        public void TestDuration(string startString, string endString, int? expectedDurationSeconds)
        {
            var start = DateTimeOffset.Parse(startString);
            var end = DateTimeOffset.Parse(endString);
            var pts = new PlainTimeSlice()
            {
                Start = start,
                End = end
            };
            var actualDuration = pts.Duration?.TotalSeconds;
            Assert.AreEqual(actualDuration, expectedDurationSeconds);
        }
    }
}