namespace RichillCapital.Max.Models;

public sealed record MarketResponse
{
    [JsonProperty("id")]
    public string Id { get; init; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; init; } = string.Empty;

    [JsonProperty("market_status")]
    public string Status { get; init; } = string.Empty;

    [JsonProperty("base_unit")]
    public string BaseUnit { get; init; } = string.Empty;

    [JsonProperty("base_unit_precision")]
    public int BaseUnitPrecision { get; init; }

    [JsonProperty("min_base_amount")]
    public decimal MinBaseAmount { get; init; }

    [JsonProperty("quote_unit")]
    public string QuoteUnit { get; init; } = string.Empty;

    [JsonProperty("quote_unit_precision")]
    public int QuoteUnitPrecision { get; init; }

    [JsonProperty("min_quote_amount")]
    public decimal MinQuoteAmount { get; init; }

    [JsonProperty("m_wallet_supported")]
    public bool MWalletSupported { get; init; }
}