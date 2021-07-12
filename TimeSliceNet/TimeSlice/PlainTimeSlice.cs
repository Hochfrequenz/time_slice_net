using System;
using TimeSlice.Converters;

namespace TimeSlice
{
    /// <summary>
    /// A plain time slice is the simplest way to model a time slice.
    /// </summary>
    public class PlainTimeSlice : ITimeSlice, IEquatable<PlainTimeSlice>
    {
        /// <summary>
        /// <inheritdoc cref="ITimeSlice.Start"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("start")]
        [System.Text.Json.Serialization.JsonConverter(typeof(EnforceTimeZoneOffsetConverter))]
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// <inheritdoc cref="ITimeSlice.End"/>
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("end")]
        [System.Text.Json.Serialization.JsonConverter(typeof(NullableEnforceTimeZoneOffsetConverter))]
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// the length of the time slice
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public TimeSpan? Duration => End - Start;

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.IsOpen() ? $"Open time slice [{Start:O} to infinity" : $"Time slice [{Start:O} - {End:O})";
        }

#pragma warning disable 8632
        /// <summary>
        /// Two time slices are identical, if <see cref="Start"/> and <see cref="End"/> are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
#pragma warning restore 8632
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlainTimeSlice)obj);
        }

        /// <inheritdoc />
        public bool Equals(PlainTimeSlice other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start.Equals(other.Start) && Nullable.Equals(End, other.End);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
    }
}