using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

public abstract class Logger : IDisposable
{
    private static readonly BlockingCollection<Action> _logQueue = new BlockingCollection<Action>();
    private static Thread _logThread;

    protected int logLevel;
    protected Logger nextLogger;
    public void SetNextLogger(Logger nextLogger) =>
        this.nextLogger = nextLogger;

    public Logger()
    {
        _logThread ??= new Thread(ProcessLogQueue)
        {
            Name = "LOGGING",
            IsBackground = true
        };
        if (!_logThread.IsAlive) _logThread.Start();
    }

    private void ProcessLogQueue()
    {
        foreach (Action action in _logQueue.GetConsumingEnumerable())
            action();
    }

    public void Log(string message, int level)
    {
        if (logLevel <= level) {
            _logQueue.Add(() => write(message));
        }
        nextLogger?.Log(message, level);
    }
    public void Info(string message) => Log(message, INFO);
    public void Debug(string message) => Log(message, DEBUG);
    public void Warn(string message) => Log(message, WARN);
    public void Error(string message) => Log(message, ERROR);

    protected abstract void write(string message);

    public void Dispose()
    {
        _logQueue.CompleteAdding();
        _logThread.Join();
        _logQueue.Dispose();
    }

    public static int INFO = 1;
    public static int DEBUG = 2;
    public static int WARN = 3;
    public static int ERROR = 4;
}