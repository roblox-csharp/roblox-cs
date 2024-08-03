using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.CodeGeneration;

public enum LuauIterator
{
    Pairs,
    IndexedPairs,
    Next,
    None
}

public class LuauWriter
{
    private WritableTextBuffer _buffer;

    public LuauWriter()
    {
        _buffer = new WritableTextBuffer();
    }

    public void WriteIterator(string indexVariableName, string valueVariableName, LuauWriter writer, string tableName, LuauIterator iteratorFunction)
    {
        var iterator = iteratorFunction switch
        {
            LuauIterator.Pairs => "pairs",
            LuauIterator.IndexedPairs => "ipairs",
            LuauIterator.Next => "next, ",
            LuauIterator.None => ""
        };

        _buffer.WriteLine((string?)null);
        _buffer.WriteLine($"for {indexVariableName}, {valueVariableName} in {iterator}({tableName}) do");
        _buffer.IncreaseIndent();
        _buffer.Write(writer.ToString());
        _buffer.DecreaseIndentation();
        _buffer.WriteLine("end");
    }

    public void WriteConditionalStatement(string condition, LuauWriter trueWriter, LuauWriter falseWriter)
    {
        _buffer.WriteLine((string?)null);
        _buffer.WriteLine($"if ({condition}) then");
        _buffer.IncreaseIndent();
        _buffer.Write(trueWriter.ToString());
        _buffer.DecreaseIndentation();
        _buffer.WriteLine("else");
        _buffer.IncreaseIndent();
        _buffer.Write(falseWriter.ToString());
        _buffer.DecreaseIndentation();
        _buffer.WriteLine("end");
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

    public override string? ToString()
    {
        return _buffer.ToString();
    }
}