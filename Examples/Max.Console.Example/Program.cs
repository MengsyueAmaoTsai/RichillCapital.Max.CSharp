

using RichillCapital.Max;

MaxDataClient restApi = new();


await restApi.GetServerTimeAsync();
var markets = await restApi.GetMarketsAsync();


foreach (var market in markets)
{
    Console.WriteLine($"{market}");
}