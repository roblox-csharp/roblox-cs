using Microsoft.CodeAnalysis;

namespace RobloxCS.Shared;

public class CleanExitException : Exception
{
    public CleanExitException(string message) : base(message)
    {
        if (!Logger.Exit) return;
        Environment.Exit(1);
    }
}

public static class Logger
{
    public static bool Exit { get; set; } = true;
    private const string _compilerError = " (roblox-cs compiler error)";
    
    public static void Ok(string message)
    {
        Log(message, ConsoleColor.Green, "OK");
    }

    public static void Info(string message)
    {
        Log(message, ConsoleColor.Cyan, "INFO");
    }

    public static CleanExitException Error(string message)
    {
        Log(message, ConsoleColor.Red, "ERROR");
        return new CleanExitException(message);
    }
    
    public static CleanExitException CompilerError(string message, SyntaxNode node) =>
        CodegenError(node, message + _compilerError);
    
    public static CleanExitException CompilerError(string message, SyntaxToken token) =>
        CodegenError(token, message + _compilerError);

    public static CleanExitException CompilerError(string message) =>
        Error(message + _compilerError);

    public static CleanExitException CodegenError(SyntaxToken token, string message)
    {
        var lineSpan = token.GetLocation().GetLineSpan();
        return Error($"{message}\n\t- {FormatLocation(lineSpan)}");
    }

    public static void CodegenWarning(SyntaxToken token, string message)
    {
        var lineSpan = token.GetLocation().GetLineSpan();
        Warn($"{message}\n\t- {FormatLocation(lineSpan)}");
    }

    public static CleanExitException UnsupportedError(SyntaxNode node, string subject, bool useIs = false, bool useYet = true) =>
        UnsupportedError(node.GetFirstToken(), subject, useIs, useYet);
    
    public static CleanExitException UnsupportedError(SyntaxToken token, string subject, bool useIs = false, bool useYet = true)
    {
        return CodegenError(token, $"{subject} {(useIs ? "is" : "are")} not {(useYet ? "yet " : "")}supported, sorry!");
    }

    public static CleanExitException CodegenError(SyntaxNode node, string message)
    {
        return CodegenError(node.GetFirstToken(), message);
    }

    public static void CodegenWarning(SyntaxNode node, string message)
    {
        CodegenWarning(node.GetFirstToken(), message);
    }

    public static void HandleDiagnostic(Diagnostic diagnostic)
    {
        HashSet<string> ignoredCodes = ["CS7022", "CS0017" /* more than one entry point */];
        if (ignoredCodes.Contains(diagnostic.Id)) return;

        var lineSpan = diagnostic.Location.GetLineSpan();
        var diagnosticMessage = $"{diagnostic.Id}: {diagnostic.GetMessage()}";
        var location = $"\n\t- {FormatLocation(lineSpan)}";
        switch (diagnostic.Severity)
        {
            case DiagnosticSeverity.Error:
            {
                Error(diagnosticMessage + location);
                break;
            }
            case DiagnosticSeverity.Warning:
            {
                if (diagnostic.IsWarningAsError)
                {
                    Error(diagnosticMessage + location);
                }
                else
                {
                    Warn(diagnosticMessage + location);
                }
                break;
            }
            case DiagnosticSeverity.Info:
            {
                Info(diagnosticMessage);
                break;
            }
        }

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

    private static string FormatLocation(FileLinePositionSpan lineSpan) =>
        $"{(lineSpan.Path == "" ? "<anonymous>" : lineSpan.Path)}:{lineSpan.StartLinePosition.Line + 1}:{lineSpan.StartLinePosition.Character + 1}";
}