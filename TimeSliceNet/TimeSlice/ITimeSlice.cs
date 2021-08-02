using System;
using System.ComponentModel.DataAnnotations;

namespace TimeSlice
{
    /// <summary>
    ///     A time slice is something that has a start and an optional end.
    /// </summary>
    public interface ITimeSlice : IValidatableObject
    {
        /// <summary>
        ///     The inclusive start date
        /// </summary>
        public DateTimeOffset Start { get; }

        /// <summary>
        ///     Exclusive end date.
        /// </summary>
        /// <remarks>null = "open" time slice</remarks>
        public DateTimeOffset? End { get; }
    }
}