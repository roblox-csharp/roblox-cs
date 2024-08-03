using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RobloxCS.CodeGeneration;

public enum LuauIterator
{
    Pairs,
    IndexedPairs,
    Next,
    None
}

public struct ConditionalStatement
{
    public string Condition;
    public LuauWriter ConditionSuccessful;
}

public enum LuaType
{
    Nil,
    Boolean,
    Number,
    String,
    Table,
    Function,
    Userdata,
    Thread,
}

public static class LuaTypeUtilities
{
    public static string LuaTypeToString(this LuaType type)
    {
        return type switch
        {
            LuaType.Nil => "nil",
            LuaType.Boolean => "boolean",
            LuaType.Number => "number",
            LuaType.String => "string",
            LuaType.Table => "table",
            LuaType.Function => "function",
            LuaType.Userdata => "userdata",
            LuaType.Thread => "thread",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

public class LuauWriter
{
    private WritableTextBuffer _buffer;

    /// <summary>
    ///     Creates a new LuauWriter
    /// </summary>
    /// <param name="useStrict"></param>
    public LuauWriter(bool includeHeader = true, bool useStrict = false, int optimizationLevel = 1)
    {
        // Bound check opt
        if (optimizationLevel > 2) optimizationLevel = 2;
        if (optimizationLevel < 0) optimizationLevel = 0;

        _buffer = new WritableTextBuffer();
        if (useStrict)
            _buffer.WriteLine("--!strict");

        _buffer.WriteLine($"--!optimize {optimizationLevel}");

        if (includeHeader)
        {
            _buffer.WriteLine(
                $"""
                 --[[
                       This file was generated using roblox-cs v{Assembly.GetAssembly(typeof(LuauWriter))!.GetName().Version!} (rbxcsc)
                   
                       If you find any issues in the generated Luau code from your C#, please report them here:
                       https://github.com/roblox-csharp/roblox-cs/issues
                   ]]
                 """);
        }
    }

    public void WriteTypeCheck(string variableName, LuaType type)
    {
        _buffer.WriteLine($"assert(type({variableName}) == {type.LuaTypeToString()}, `{variableName} is not a {type.LuaTypeToString()}! Instead, it is of type {{type({variableName})}}.`)");
    }

    public void WriteMethodLuauCall(string tableName, string functionName, params string[] argumentVariableNames)
    {
        _buffer.WriteLine($"{tableName}[\"{functionName}\"]({tableName}, {string.Join(", ", argumentVariableNames)})");
    }

    public void WriteLibraryLuauCall(string tableName, string functionName, params string[] argumentVariableNames)
    {
        _buffer.WriteLine($"{tableName}[\"{functionName}\"]({string.Join(", ", argumentVariableNames)})");
    }

    public void WriteLuauCall(string functionName, params string[] argumentVariableNames)
    {
        _buffer.WriteLine($"{functionName}({string.Join(", ", argumentVariableNames)})");
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

    public void WriteConditionalStatement(ConditionalStatement[] conditions)
    {
        foreach (var condition in conditions)
        {
            _buffer.WriteLine("if {condition.Condition} then");
            _buffer.IncreaseIndent();
            _buffer.Write(condition.ConditionSuccessful.ToString());
            _buffer.DecreaseIndentation();
            _buffer.WriteLine("end");
        }
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