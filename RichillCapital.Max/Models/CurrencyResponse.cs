
namespace RichillCapital.Max.Models;

public sealed record CurrencyResponse
{
    public string Id { get; init; } = string.Empty;
    public int Precision { get; init; }

    [JsonProperty("sygna_supported")]
    public bool SygnaSupported { get; init; }
}