public static class Logger
{
    public static void Ok(string message)
    {
        Log(message, ConsoleColor.Green, "OK");
    }

    public static void Info(string message)
    {
        Log(message, ConsoleColor.Cyan, "INFO");
    }

    public static void Error(string message)
    {
        Log(message, ConsoleColor.Red, "ERROR");
        Environment.Exit(1);
    }

    public static void Warn(string message)
    {
        Log(message, ConsoleColor.Yellow, "WARN");
    }

    public static void Debug(string message)
    {
        Log(message, ConsoleColor.Magenta, "DEBUG");
    }

    private static void Log(string message, ConsoleColor color, string level)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{level}] {message}");
        Console.ForegroundColor = originalColor;
    }
}