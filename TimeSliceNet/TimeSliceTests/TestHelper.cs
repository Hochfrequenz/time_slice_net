using System;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    /// Useful methods for unit tests
    /// </summary>
    internal class TestHelper
    {
        /// <summary>
        /// Create a time slice from two parseable date time strings
        /// </summary>
        /// <param name="startString">mandatory start</param>
        /// <param name="endString">optional end (can be null)</param>
        /// <returns></returns>
        internal static PlainTimeSlice CreateTimeSlice(string startString, string endString)
        {
            if (startString is null) throw new ArgumentNullException(nameof(startString));
            var start = DateTimeOffset.Parse(startString);
            DateTimeOffset? end = string.IsNullOrWhiteSpace(endString) ? null : DateTimeOffset.Parse(endString);
            var pts = new PlainTimeSlice
            {
                Start = start,
                End = end
            };
            return pts;
        }
    }
}