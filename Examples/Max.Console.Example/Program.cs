

using RichillCapital.Max;

MaxDataClient restApi = new();

await restApi.EstablishConnectionAsync();

restApi.Ping();
restApi.SubscribeMarketStatus();


Console.ReadLine();