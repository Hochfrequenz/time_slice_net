using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (End < Start)
            {
                results.Add(new ValidationResult($"The {nameof(End)} ({End:O}) must not be earlier than the {nameof(Start)} ({Start:O})", new List<string>() { nameof(Start), nameof(End) }));
            }
            if (End.HasValue)
            {
                if (End != DateTimeOffset.MaxValue && End.Value.Minute == 59 && End.Value.Second == 59)
                {
                    // https://imgflip.com/i/5gbuqi
                    // Make sure no one tries to use "23:59:59" style pseudo-inclusive end dates.
                    // This is in fact patronizing but I can't think of any business case in which a time slice that - because of the exclusive character of the end date - does not include the last second of an hour is actually intended.
                    // The only exception is infinity.
                    // We better fail early upon validation of an object than to carry around data that without a shared/common understanding.
                    results.Add(new ValidationResult($"The {nameof(End)} {End.Value:O} has to be exclusive but it seems like it's meant to be inclusive.", new List<string> { nameof(End) }));
                }
            }
            return results;
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