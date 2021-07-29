using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeSlice.Converters
{
    /// <summary>
    ///     string extension methods, that are only used internally
    /// </summary>
    internal static class DateTimeOffsetConverterStringExtension
    {
        /// <summary>
        ///     similar to "O" but without the sub seconds
        /// </summary>
        internal const string TheOneAndOnlyDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzzz";

        /// <summary>
        ///     converts <paramref name="dateTimeString" /> to a datetime with offset 0
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns>null iff <paramref name="dateTimeString" /> is null</returns>
        /// <exception cref="FormatException">iff <paramref name="dateTimeString" /> is not parseable with an offset</exception>
        internal static DateTimeOffset? ToUtcDateTimeOffset(this string dateTimeString)
        {
            if (dateTimeString == null) return null;
            // somehow the datetime format string "O" does not work. but with the workaround below the "Z" is not recognized
            if (dateTimeString.EndsWith("Z")) dateTimeString = dateTimeString.Replace("Z", "+00:00");
            var value = DateTimeOffset.ParseExact(dateTimeString, TheOneAndOnlyDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return value.UtcDateTime;
        }
    }

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

    /// <summary>
    ///     A converter that throws a <see cref="FormatException" /> if the string that should be deserialized to a DateTimeOffset has no offset/timezone information.
    ///     We're not guessing.
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support</remarks>
    public class EnforceTimeZoneOffsetConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc />
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            var result = dateTimeString.ToUtcDateTimeOffset();
            if (!result.HasValue) throw new FormatException("The value must not be null.");
            return result.Value;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeOffsetConverterStringExtension.TheOneAndOnlyDateTimeFormat));
        }
    }

    /// <summary>
    ///     A converter that throws a <see cref="FormatException" /> if the string that should be deserialized to a DateTimeOffset? has no offset/timezone information.
    ///     We're not guessing.
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support</remarks>
    public class NullableEnforceTimeZoneOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        /// <inheritdoc />
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            return dateTimeString.ToUtcDateTimeOffset();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteStringValue(value.Value.ToString(DateTimeOffsetConverterStringExtension.TheOneAndOnlyDateTimeFormat));
        }
    }
}