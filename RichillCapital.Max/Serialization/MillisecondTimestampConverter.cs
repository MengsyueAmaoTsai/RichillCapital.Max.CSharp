
using Newtonsoft.Json.Converters;

namespace RichillCapital.Max.Serialization;

public class MillisecondTimestampConverter : DateTimeConverterBase
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is null)
            return null;

        long milliseconds = Convert.ToInt64(reader.Value);
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        return dateTimeOffset;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            long milliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
            writer.WriteValue(milliseconds);
        }
        else
        {
            throw new InvalidOperationException("Expected DateTimeOffset value.");
        }
    }
}
