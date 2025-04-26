using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RobloxCS.Luau;
using RobloxCS.Transformers;

namespace RobloxCS.Tests.TransformerTests;

public class MainTransformerTest
{
    [Fact]
    public void Transforms_PrimaryConstructors()
    {
        const string source = """
                              class MyClass(int abc, string def)
                              {
                                  public int Abc { get; } = abc;
                                  public string Def { get; } = def;
                              }
                              """;
        
        var compilationUnit = Transform(source);
        var classDecl = compilationUnit.Members.OfType<ClassDeclarationSyntax>().Single();
        Assert.Null(classDecl.ParameterList); // removed primary ctor

        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>().ToArray();
        Assert.Equal(2, properties.Length);
        Assert.Contains(properties, p => p.Identifier.Text == "Abc");
        Assert.Contains(properties, p => p.Identifier.Text == "Def");
        Assert.All(properties, p => Assert.Null(p.Initializer)); // props using primary params have no initializer

        var constructors = classDecl.Members.OfType<ConstructorDeclarationSyntax>().ToList();
        Assert.Single(constructors);
        
        var constructor = constructors.First();
        Assert.Equal("MyClass", constructor.Identifier.Text);

        var parameters = constructor.ParameterList.Parameters;
        Assert.Equal(2, parameters.Count);
        Assert.Equal("abc", parameters[0].Identifier.Text);
        Assert.Equal("def", parameters[1].Identifier.Text);

        var statements = constructor.Body!.Statements.OfType<ExpressionStatementSyntax>().ToArray();
        Assert.Equal(2, statements.Length);

        var assignments = statements.Select(s => s.Expression).OfType<AssignmentExpressionSyntax>().ToArray();
        Assert.All(assignments, assign =>
        {
            Assert.Equal(SyntaxKind.SimpleAssignmentExpression, assign.Kind());
            Assert.IsType<MemberAccessExpressionSyntax>(assign.Left);
            Assert.IsType<IdentifierNameSyntax>(assign.Right);
        });

        var assignedProperties = assignments.Select(a =>
                                                        ((MemberAccessExpressionSyntax)a.Left).Name.Identifier.Text
                                                   ).ToArray();

        Assert.Contains("Abc", assignedProperties);
        Assert.Contains("Def", assignedProperties);
    }
    
    [Fact]
    public void Transforms_FileScopedNamespaces()
    {
        var compilationUnit = Transform("namespace Abc;");
        Assert.Single(compilationUnit.Members);
        Assert.IsType<NamespaceDeclarationSyntax>(compilationUnit.Members.First());

        var @namespace = (NamespaceDeclarationSyntax)compilationUnit.Members.First();
        Assert.Equal("Abc", @namespace.Name.ToString());
    }

    [Fact]
    public void AddsExtraUsings()
    {
        var compilationUnit = Transform("");
        Assert.Equal(5, compilationUnit.Usings.Count);

        var usingSystemCollectionsGeneric = compilationUnit.Usings[0];
        var usingSystemCollections = compilationUnit.Usings[1];
        var usingSystemLinq = compilationUnit.Usings[2];
        var usingRoblox = compilationUnit.Usings[3];
        var usingRobloxGlobals = compilationUnit.Usings[4];
        Assert.Equal("System.Collections.Generic", usingSystemCollectionsGeneric.Name?.ToString());
        Assert.Equal("System.Collections", usingSystemCollections.Name?.ToString());
        Assert.Equal("System.Linq", usingSystemLinq.Name?.ToString());
        Assert.Equal("Roblox", usingRoblox.Name?.ToString());
#pragma warning disable xUnit2002
        Assert.NotNull(usingRobloxGlobals.StaticKeyword);
#pragma warning restore xUnit2002
        Assert.Equal("Roblox.Globals", usingRobloxGlobals.Name?.ToString());
    }

    private static CompilationUnitSyntax Transform(string source)
    {
        var cleanTree = SyntaxFactory.ParseSyntaxTree(source);
        var transform = BuiltInTransformers.Main();
        var transformedTree = transform(cleanTree, new TransformState(), new ConfigData());

        return transformedTree.GetCompilationUnitRoot();
    }
}