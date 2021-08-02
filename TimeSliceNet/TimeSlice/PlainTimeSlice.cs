using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TimeSlice.Converters;

namespace TimeSlice
{
    /// <summary>
    ///     A plain time slice is the simplest way to model a time slice.
    /// </summary>
    public class PlainTimeSlice : ITimeSlice, IEquatable<PlainTimeSlice>
    {
        private DateTimeOffset? _end;

        private DateTimeOffset _start;

        /// <summary>
        ///     the length of the time slice
        /// </summary>
        [JsonIgnore]
        public TimeSpan? Duration => End - Start;

        /// <inheritdoc />
        public bool Equals(PlainTimeSlice other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start.Equals(other.Start) && Nullable.Equals(End, other.End);
        }

        /// <summary>
        ///     <inheritdoc cref="ITimeSlice.Start" />
        /// </summary>
        [JsonConverter(typeof(EnforceTimeZoneOffsetConverter))]
        public DateTimeOffset Start
        {
            get => _start;
            set => _start = value.StripSubSecond();
        }

        /// <summary>
        ///     <inheritdoc cref="ITimeSlice.End" />
        /// </summary>
        [JsonConverter(typeof(NullableEnforceTimeZoneOffsetConverter))]
        public DateTimeOffset? End
        {
            get => _end;
            set => _end = value.StripSubSecond();
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (End < Start)
                results.Add(new ValidationResult($"The {nameof(End)} ({End:O}) must not be earlier than the {nameof(Start)} ({Start:O})",
                    new List<string> { nameof(Start), nameof(End) }));
            if (!End.HasValue) return results;
            if (End != DateTimeOffset.MaxValue && End.Value.Minute == 59 && End.Value.Second == 59)
                // https://imgflip.com/i/5gbuqi
                // Make sure no one tries to use "23:59:59" style pseudo-inclusive end dates.
                // This is in fact patronizing but I can't think of any business case in which a time slice that - because of the exclusive character of the end date - does not include the last second of an hour is actually intended.
                // The only exception is infinity.
                // We better fail early upon validation of an object than to carry around data that without a shared/common understanding.
                results.Add(new ValidationResult($"The {nameof(End)} {End.Value:O} has to be exclusive but it seems like it's meant to be inclusive.",
                    new List<string> { nameof(End) }));
            return results;
        }

        /// <summary>
        ///     <inheritdoc cref="object.ToString" />
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.IsOpen() ? $"Open time slice [{Start:O} to infinity" : $"Time slice [{Start:O} - {End:O})";
        }

#pragma warning disable 8632
        /// <summary>
        ///     Two time slices are identical, if <see cref="Start" /> and <see cref="End" /> are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
#pragma warning restore 8632
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PlainTimeSlice)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
    }
}