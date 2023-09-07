

using RichillCapital.Max.Models;

namespace RichillCapital.Max;

public sealed class MaxDataClient
{
    private readonly HttpClient _httpClient;

    public MaxDataClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://max-api.maicoin.com")
        };
    }

    public async Task<DateTimeOffset> GetServerTimeAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/timestamp");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(content));
        return dateTime;
    }

    public async Task<IReadOnlyCollection<MarketResponse>> GetMarketsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/markets");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        var markets = JsonConvert.DeserializeObject<IEnumerable<MarketResponse>>(content)
            ?? Array.Empty<MarketResponse>();
        return markets.ToArray().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<CurrencyResponse>> GetCurrenciesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/currencies");
        var response = await _httpClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        var markets = JsonConvert.DeserializeObject<IEnumerable<CurrencyResponse>>(content)
            ?? Array.Empty<CurrencyResponse>();
        return markets.ToArray().AsReadOnly();
    }
}
