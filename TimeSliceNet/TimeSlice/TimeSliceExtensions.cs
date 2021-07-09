using System;

namespace TimeSlice
{
    /// <summary>
    /// A static class with extension methods for objects/classes that implement <see cref="ITimeSlice"/>
    /// </summary>
    public static class TimeSliceExtensions
    {
        /// <summary>
        /// A time slice is called "open" if it has either no end or the end is infinity.
        /// </summary>
        /// <param name="timeSlice"></param>
        /// <returns>true if open</returns>
        public static bool IsOpen(this ITimeSlice timeSlice) => !timeSlice.End.HasValue || timeSlice.End.Value == DateTimeOffset.MaxValue;


        /// <summary>
        /// check if a datetime is in side a time slice.
        /// </summary>
        /// <param name="timeSlice">any time slice</param>
        /// <param name="dt">a datetime</param>
        /// <returns>true iff <paramref name="dt"/> is inside <paramref name="timeSlice"/></returns>
        public static bool Overlaps(this ITimeSlice timeSlice, DateTimeOffset dt)
        {
            if (timeSlice.IsOpen()) return timeSlice.Start <= dt;
            return timeSlice.Start <= dt && timeSlice.End.HasValue && timeSlice.End.Value > dt;
            // remember: the end date is _exclusive_
        }

        /// <summary>
        /// check if a time slice overlaps with another time slice
        /// </summary>
        /// <param name="timeSlice"></param>
        /// <param name="other"></param>
        /// <returns>true if <paramref name="timeSlice"/> and <paramref name="other"/> share a finite time span</returns>
        public static bool Overlaps(this ITimeSlice timeSlice, ITimeSlice other)
        {
            if (timeSlice.IsOpen() && other.IsOpen())
            {
                return true; // they overlap in infinity
            }
            if (timeSlice.IsOpen())
            {
                // [------other---)
                //    [---time slice----------------
                return other.End > timeSlice.Start;
            }

            if (other.IsOpen())
            {
                //     [--------other--------
                //  [---time slice-------)
                return timeSlice.End > other.Start;
            }
            return timeSlice.End != null && other.End != null // <-- to satisfy the linter. It's ensured though, because both timeSlice and other are _not_ open 
                                         && timeSlice.Start < other.End.Value && timeSlice.End.Value > other.Start;
        }
    }
}