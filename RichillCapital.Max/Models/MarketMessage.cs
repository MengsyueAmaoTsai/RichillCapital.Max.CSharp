
namespace RichillCapital.Max.Models;

public sealed record MarketMessage
{
    [JsonProperty("M")]
    public string Id { get; init; } = string.Empty;

    [JsonProperty("st")]
    public string Status { get; init; } = string.Empty;

    [JsonProperty("bu")]
    public string BaseUnit { get; init; } = string.Empty;

    [JsonProperty("bup")]
    public int BaseUnitPrecision { get; init; }

    [JsonProperty("mba")]
    public decimal MinBaseAmount { get; init; }

    [JsonProperty("qu")]
    public string QuoteUnit { get; init; } = string.Empty;

    [JsonProperty("qup")]
    public int QuoteUnitPrecision { get; init; }

    [JsonProperty("mqa")]
    public decimal MinQuoteAmount { get; init; }

    [JsonProperty("mws")]
    public bool MWalletSupported { get; init; }

    [JsonProperty("gs")]
    public bool Gs { get; init; }

    [JsonProperty("gsm")]
    public string Gsm { get; init; } = string.Empty;

}