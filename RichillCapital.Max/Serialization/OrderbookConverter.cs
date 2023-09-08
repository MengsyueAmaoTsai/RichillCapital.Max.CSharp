
using Newtonsoft.Json.Linq;

using RichillCapital.Max.Events;

namespace RichillCapital.Max.Serialization;

public class OrderBookEventConverter : JsonConverter<OrderbookEvent>
{
    public override OrderbookEvent? ReadJson(JsonReader reader, Type objectType, OrderbookEvent? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject json = JObject.Load(reader);

        var timestamp = json.SelectToken("timestamp")?.Value<long>() ?? 0;

        return new()
        {
            DateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp),
            Bids = ConvertToOrderbookEntries(json["b"]) ?? Array.Empty<OrderbookEntry>(),
            Asks = ConvertToOrderbookEntries(json["a"]) ?? Array.Empty<OrderbookEntry>()
        };
    }

    private static OrderbookEntry[]? ConvertToOrderbookEntries(JToken? token)
    {
        if (token is null || token.Type == JTokenType.Null) return null;
        JArray array = (JArray)token;
        OrderbookEntry[] entries = new OrderbookEntry[array.Count];

        for (int i = 0; i < array.Count; i++)
        {
            JArray item = (JArray)array[i];
            entries[i] = new OrderbookEntry
            {
                Price = item[0]?.Value<decimal>() ?? throw new JsonSerializationException("Missing or invalid 'price' property in 'bids' or 'asks'."),
                Volume = item[1]?.Value<decimal>() ?? throw new JsonSerializationException("Missing or invalid 'volume' property in 'bids' or 'asks'.")
            };
        }
        return entries;
    }

    public override void WriteJson(JsonWriter writer, OrderbookEvent? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
