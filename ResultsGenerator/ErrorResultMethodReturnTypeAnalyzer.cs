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
    /// It reports diagnostics for implicitly typed local variables, recommending explicit type specification.
    /// </summary>
    /// <remarks>
    /// For analyzers that requires analyzing symbols or syntax nodes across compilation, see <see cref="CompilationStartedAnalyzer"/> and <see cref="CompilationStartedAnalyzerWithCompilationWideAnalysis"/>.
    /// For analyzers that requires analyzing symbols or syntax nodes across a code block, see <see cref="CodeBlockStartedAnalyzer"/>.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ErrorResultMethodReturnTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "ResultsFramework_001"; 
        private const string Title = "Invalid return type";
        public const string MessageFormat = "Method '{0}' defines ErrorResult but doesnt return a Result or Result<T> value.";
        public const string AsyncMessageFormat = "Async method '{0}' defines ErrorResult but doesnt return a Task<Result> or Task<Result<T>> value.";
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
            // Find implicitly typed variable declarations.
            if (declaration.AttributeLists.Any(x => x.Attributes.Any(a => a.Name.ToString() == "ErrorResult")))
            {
                var returnType = declaration.ReturnType;
                if (declaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword))
                {
                    if (!returnType.ToString().StartsWith("Task<Result"))
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
                else if(!returnType.ToString().StartsWith("Result"))
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