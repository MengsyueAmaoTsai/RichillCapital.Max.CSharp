

using RichillCapital.Max;
using RichillCapital.Max.Models;

MaxDataClient dataClient = new();
dataClient.Pong += HandlePong;


Console.WriteLine("|====================================|");
Console.WriteLine("|    MaxDataClient Console Example   |");
Console.WriteLine("|====================================|");
Console.WriteLine();



Console.WriteLine("|====================================|");
Console.WriteLine("|              Starting              |");
Console.WriteLine("|====================================|");
Console.WriteLine();

await dataClient.EstablishConnectionAsync();

// await Task.Delay(2000);
// await dataClient.CloseConnectionAsync();

Console.ReadKey();
Console.WriteLine("|====================================|");
Console.WriteLine("|              Stopped               |");
Console.WriteLine("|====================================|");


static void HandlePong(object? sender, PongEvent e)
{
    Console.WriteLine($"Pong from server - {e}");
}