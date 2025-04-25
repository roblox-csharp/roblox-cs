using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace RobloxCS.Tests;

public class LuauTests(ITestOutputHelper testOutputHelper)
{
    private readonly string _cwd =
        Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                                                                        .Location))))!;

    [Theory]
    [InlineData("RuntimeLibTest")]
    public void LuauTests_Pass(string scriptName)
    {
        var lunePath = Path.GetFullPath("lune" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""), _cwd);
        var runScriptArguments = $"run {scriptName}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = lunePath,
                Arguments = runScriptArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _cwd
            }
        };

        try
        {
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            testOutputHelper.WriteLine($"{scriptName}.luau Errors:");
            testOutputHelper.WriteLine(error);
            Assert.True(string.IsNullOrWhiteSpace(error));
            testOutputHelper.WriteLine($"{scriptName}.luau Output:");
            testOutputHelper.WriteLine(output);
            Assert.True(string.IsNullOrWhiteSpace(output));
            Assert.Equal(0, process.ExitCode);
        }
        finally
        {
            process.Dispose();
        }
    }
}