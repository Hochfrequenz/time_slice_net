using System;
using System.Globalization;
using System.Text.Json;

namespace TimeSlice.Converters
{
    /// <summary>
    /// A converter that throws a <see cref="FormatException"/> if the string that should be deserialized to a DateTimeOffset has no offset/timezone information.
    /// We're not guessing.
    /// </summary>
    /// <remarks>https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support</remarks>
    public class EnforceTimeZoneOffsetConverter : System.Text.Json.Serialization.JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc />
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            // somehow the datetime format string "O" does not work. but with the workaround below the "Z" is not recognized
            if (dateTimeString.EndsWith("Z"))
            {
                dateTimeString = dateTimeString.Replace("Z", "+00:00");
            }
            var value = DateTimeOffset.ParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:sszzzz", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return value.UtcDateTime;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}