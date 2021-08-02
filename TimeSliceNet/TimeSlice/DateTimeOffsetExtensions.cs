using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TimeSliceTests")]

namespace TimeSlice
{
    /// <summary>
    ///     extensions for <see cref="DateTimeOffset" />
    /// </summary>
    internal static class DateTimeOffsetExtensions
    {
        /// <summary>
        ///     removes any <see cref="DateTimeOffset.Millisecond" /> and ticks from <paramref name="dto" />
        /// </summary>
        /// <param name="dto"></param>
        internal static DateTimeOffset? StripSubSecond(this DateTimeOffset? dto)
        {
            var result = dto?.Subtract(TimeSpan.FromTicks(dto.Value.Ticks % TimeSpan.TicksPerSecond));
            return result;
        }

        internal static DateTimeOffset StripSubSecond(this DateTimeOffset dto)
        {
            var result = dto.Subtract(TimeSpan.FromTicks(dto.Ticks % TimeSpan.TicksPerSecond));
            return result;
        }
    }
}