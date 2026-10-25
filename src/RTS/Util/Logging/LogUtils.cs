using Microsoft.VisualBasic.Logging;

public static class LogUtils
{
    private static Logger logger;
    public static Logger GetLogger() => logger;
    public static void SetLogger(Logger logger) =>
        LogUtils.logger = logger;
}