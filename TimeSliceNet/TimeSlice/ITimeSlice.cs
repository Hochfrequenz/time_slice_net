using System;

namespace TimeSlice
{
    /// <summary>
    /// A time slice is something that has a start and an optional end.
    /// </summary>
    public interface ITimeSlice
    {
        /// <summary>
        /// The inclusive start date
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// Exclusive end date.
        /// </summary>
        /// <remarks>null = "open" time slice</remarks>
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// The duration describes the length of a time slice.
        /// It is null if and only if <see cref="End"/> is null. 
        /// </summary>
        public TimeSpan? Duration { get; }
    }
}