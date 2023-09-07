

using RichillCapital.Max;

MaxDataClient restApi = new();


await restApi.GetServerTimeAsync();
var markets = await restApi.GetMarketsAsync();
var currencies = await restApi.GetCurrenciesAsync();


foreach (var c in currencies)
{
    Console.WriteLine($"{c}");
}