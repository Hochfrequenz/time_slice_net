using System;

namespace TimeSlice
{
    /// <summary>
    /// A static class with extension methods for objects/classes that implement <see cref="ITimeSlice"/>
    /// </summary>
    public static class TimeSliceExtensions
    {
        /// <summary>
        /// check if a datetime is in side a time slice.
        /// </summary>
        /// <param name="timeSlice">any time slice</param>
        /// <param name="dt">a datetime</param>
        /// <returns>true iff <paramref name="dt"/> is inside <paramref name="timeSlice"/></returns>
        public static bool Overlaps(this ITimeSlice timeSlice, DateTimeOffset dt)
        {
            if (!timeSlice.End.HasValue) return timeSlice.Start <= dt;
            return timeSlice.Start <= dt && timeSlice.End.Value > dt;
            // remember: the end date is _exclusive_
        }
    }
}