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
        public DateTimeOffset Start { get; }

        /// <summary>
        /// Exclusive end date.
        /// </summary>
        /// <remarks>null = "open" time slice</remarks>
        public DateTimeOffset? End { get; }
    }
}