namespace Collector_AWS;

public static class Logger
{
    public static void log(string? message)
    {
        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} : {message}");
    }
}