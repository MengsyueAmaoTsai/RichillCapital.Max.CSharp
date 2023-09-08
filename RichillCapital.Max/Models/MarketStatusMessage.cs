
namespace RichillCapital.Max.Models;

public sealed record MarketStatusMessage
{
    [JsonProperty("M")]
    public string MarketId { get; init; } = string.Empty;

    [JsonProperty("st")]
    public string Status { get; init; } = string.Empty;

    [JsonProperty("bu")]
    public string BaseAsset { get; init; } = string.Empty;

    [JsonProperty("bup")]
    public int BaseAssetPrecision { get; init; }

    [JsonProperty("mba")]
    public decimal MinBaseAmount { get; init; }

    [JsonProperty("qu")]
    public string QuoteAsset { get; init; } = string.Empty;

    [JsonProperty("qup")]
    public int QuoteAssetPrecision { get; init; }
    [JsonProperty("mqa")]
    public decimal MinQuoteAmount { get; init; }
    [JsonProperty("mws")]
    public bool MWalletSupported { get; init; }

    [JsonProperty("gs")]
    public bool Gs { get; init; }

    [JsonProperty("gsm")]
    public string Gsm { get; init; } = string.Empty;
}