using Microsoft.CodeAnalysis;

namespace RobloxCS
{
    internal static class Logger
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

        public static void CompilerError(string message)
        {
            Error($"{message} (compiler error)");
        }

        public static void CodegenError(SyntaxToken token, string message)
        {
            var lineSpan = token.GetLocation().GetLineSpan();
            Error($"{message}\n\t- {Utility.FormatLocation(lineSpan)}");
        }

        public static void CodegenError(SyntaxNode node, string message)
        {
            CodegenError(node.GetFirstToken(), message);
        }

        public static void HandleDiagnostic(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetLineSpan();
            var diagnosticMessage = $"{diagnostic.Id}: {diagnostic.GetMessage()}";
            var location = $"\n\t- {Utility.FormatLocation(lineSpan)}";
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
            if (!Utility.IsDebug()) return;
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
}