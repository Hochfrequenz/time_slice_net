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
        public TimeSpan? Duration => End - Start;

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.IsOpen() ? $"Open time slice [{Start:O} to infinity" : $"Time slice [{Start:O} - {End:O})";
        }

        /// <summary>
        /// Two time slices are identical, if <see cref="Start"/> and <see cref="End"/> are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
#pragma warning disable 8632
        public override bool Equals(object? obj)
#pragma warning restore 8632
        {
            return obj switch
            {
                null => false,
                PlainTimeSlice other => Start.Equals(other.Start) && Nullable.Equals(End, other.End),
                _ => false
            };
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
    }
}