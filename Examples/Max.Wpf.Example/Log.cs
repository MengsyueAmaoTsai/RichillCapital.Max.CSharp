using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max.Wpf.Example;

public sealed record Log
{
    public DateTimeOffset DateTime { get; init; }
    public string Level { get; init; } 
    public string Message { get; init; }

    public static Log Info(string message) => Create("INFO", message);

    public static Log Create(string level, string message) =>
        new() { DateTime = DateTimeOffset.UtcNow, Level = level, Message = message };
}
