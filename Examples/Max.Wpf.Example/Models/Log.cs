using System;

namespace Max.Wpf.Example.Models;

public sealed record Log
{
    public DateTimeOffset DateTime { get; init; }
    public string Level { get; init; }
    public string Message { get; init; }

    private Log(string level, string message)
    {
        DateTime = DateTimeOffset.UtcNow;
        Level = level;
        Message = message;
    }

    public static Log Info(string message) => Create("INFO", message);
    public static Log Error(string message) => Create("ERROR", message);
    public static Log Create(string level, string message) => new(level, message);
}
