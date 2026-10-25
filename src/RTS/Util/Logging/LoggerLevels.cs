using System;
using System.Diagnostics;
using System.Threading;

internal class LoggerInfo : Logger
{
    public LoggerInfo() => logLevel = INFO;

    protected override void write(string message)
    {
        
    }
}
internal class LoggerDebug : Logger
{
    public LoggerDebug() => logLevel = DEBUG;

    protected override void write(string message)
    {
        
    }
}
internal class LoggerWarn : Logger
{
    public LoggerWarn() => logLevel = WARN;

    protected override void write(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("WARNING: {0}", message);
        Console.ResetColor();
    }
}
internal class LoggerError : Logger
{
    public LoggerError() => logLevel = ERROR;

    protected override void write(string message)
    {
        
    }
}