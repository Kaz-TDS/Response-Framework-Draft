using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TDS.ResultsGenerator
{
    /// <summary>
    /// Analyzer for reporting syntax node diagnostics.
    /// It reports diagnostics for ErrorResult attribute declarations, it validates that the method with ErrorResult returns one of the allowed types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ErrorResultMethodReturnTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "ResultsFramework_001"; 
        private const string Title = "Invalid return type";
        private const string MessageFormat = "Method '{0}' defines ErrorResult but doesnt return a Result or Result<T> value.";
        private const string AsyncMessageFormat = "Async method '{0}' defines ErrorResult but doesnt return a Task<Result> or Task<Result<T>> value.";
        private const string Description = "Invalid return type";

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                Id,
                Title,
                MessageFormat,
                "Stateless",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)context.Node;
            // Look for ErrorResult attributes
            if (declaration.AttributeLists.Any(x => x.Attributes.Any(a => a.Name.ToString() == "ErrorResult")))
            {
                var returnType = declaration.ReturnType;
                var returnTypeIdentity = returnType.ToString();
                if (declaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword))
                {
                    if (!returnTypeIdentity.StartsWith("Task<Result") && !returnTypeIdentity.StartsWith("UniTask<Result"))
                    {
                        var rule = new DiagnosticDescriptor(
                            Id,
                            Title,
                            AsyncMessageFormat,
                            "stateless",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true,
                            description: Description);
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                rule,
                                declaration.GetLocation(),
                                declaration.Identifier.ValueText));
                    }
                }
                else if(!returnTypeIdentity.StartsWith("Result") && !returnTypeIdentity.StartsWith("UniTask<Result"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            declaration.GetLocation(),
                            declaration.Identifier.ValueText));
                }
            }
        }
    }
}