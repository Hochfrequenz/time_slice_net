using System;

namespace TimeSlice
{
    /// <summary>
    /// A plain time slice is the simplest way to model a time slice.
    /// </summary>
    public class PlainTimeSlice : ITimeSlice
    {
        /// <summary>
        /// <inheritdoc cref="ITimeSlice.Start"/>
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// <inheritdoc cref="ITimeSlice.End"/>
        /// </summary>
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// <inheritdoc cref="ITimeSlice.Duration"/>
        /// </summary>
        public TimeSpan? Duration => End.HasValue ? End - Start : null;
    }
}