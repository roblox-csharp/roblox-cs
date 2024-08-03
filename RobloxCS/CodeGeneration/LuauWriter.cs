using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.CodeGeneration;

public class LuauWriter
{
    private WritableTextBuffer _buffer;

    public LuauWriter()
    {
        _buffer = new WritableTextBuffer();
    }

    public void WriteRequire(string requirePath)
    {
        _buffer.WriteLine($"require({requirePath})");
    }

    public void DefineVariable(string variableName)
    {
        WriteVariable(variableName, null);
    }

    public void WriteVariable(string variableName, string? defaultVariableValue = null)
    {
        WriteVariableWithTypeAnnotation(variableName, null, defaultVariableValue);
    }

    public void WriteReturn(string variableName, TypeSyntax? type)
    {
        _buffer.WriteLine(
            type == null ?
                $"return {variableName} :: any" :
                $"{variableName} :: {Utility.GetMappedType(type.ToString())}"
        );
    }

    public void WriteVariableWithTypeAnnotation(string variableName, TypeSyntax? type, string? defaultVariableValue)
    {
        var typeNote = "";
        if (type is { IsVar: false })
        {
            typeNote = $": {Utility.GetMappedType(type.ToString())}";
        }

        // Assume that defining the variable is what is intended.
        _buffer.WriteLine(
            defaultVariableValue == null ?
                $"{variableName}{typeNote}" :
                $"{variableName}{typeNote} = {defaultVariableValue}"
        );
    }
}