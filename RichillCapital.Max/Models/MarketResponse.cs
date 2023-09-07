
namespace RichillCapital.Max.Models;

public sealed record MarketResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    [JsonProperty("market_status")]
    public string Status { get; init; } = string.Empty;

    [JsonProperty("base_unit")]
    public string BaseAsset { get; init; } = string.Empty;

    [JsonProperty("base_unit_precision")]
    public int BaseAssetPrecision { get; set; }

    [JsonProperty("min_base_amount")]
    public decimal MinBaseAssetAmount { get; init; }

    [JsonProperty("quote_unit")]
    public string QuoteAsset { get; init; } = string.Empty;

    [JsonProperty("quote_unit_precision")]
    public int QuoteAssetPrecision { get; set; }

    [JsonProperty("min_quote_amount")]
    public decimal MinQuoteAssetAmount { get; init; }

    [JsonProperty("m_wallet_supported")]
    public bool MWalletSupported { get; init; }
}