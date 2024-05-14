using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using Document = Microsoft.CodeAnalysis.Document;

namespace TDS.ResultsGenerator
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class ErrorResultMethodReturnTypeFix : CodeFixProvider
    {
        private const string EquivalenceKey = "Invalid return type fix";
        public override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(ErrorResultMethodReturnTypeAnalyzer.Id);
        
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(CodeAction.Create(
                    title: "Fix return type",
                    createChangedDocument: c => 
                        FixReturnTypeAsync(context.Document, diagnostic.Location.SourceSpan, c),
                    equivalenceKey: EquivalenceKey),
                    diagnostic);
            }
            return Task.CompletedTask;
        }

        private async Task<Document> FixReturnTypeAsync(Document document, 
            TextSpan sourceSpan,
            CancellationToken cancellationToken)
        {
            var tree = (await document.GetSyntaxTreeAsync(cancellationToken))!;
            var root = await tree.GetRootAsync(cancellationToken);
            var token = root.FindToken(sourceSpan.Start);
            SyntaxNode methodNode = default;
            TypeSyntax identifier = default;
            if (token.Kind() != SyntaxKind.MethodDeclaration)
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                methodNode = token.Parent;
                while (methodNode.Kind() != SyntaxKind.MethodDeclaration)
                {
                    methodNode = methodNode.Parent;
                }

                var method = (MethodDeclarationSyntax)methodNode;

                identifier = GetReplacementTypeNode(method);
            }
            
            SyntaxNode nodeToReplace = GetReturnTypeNode(methodNode);
            
            return document.WithSyntaxRoot(root.ReplaceNode(nodeToReplace, identifier));
        }

        private static SyntaxNode GetReturnTypeNode(SyntaxNode methodNode)
        {
            return methodNode.ChildNodes().First(x => !(x is AttributeListSyntax) && !(x is ParameterListSyntax) && !(x is BlockSyntax));
        }

        private static TypeSyntax GetReplacementTypeNode(MethodDeclarationSyntax method)
        {
            TypeSyntax identifier;
            string source;
            if (method.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword))
            {
                string returnType = method.ReturnType.ToString();
                if (returnType.StartsWith("Task"))
                {
                    returnType = returnType.Replace("Task", "");
                    var startIndex = returnType.IndexOf("<");
                    var endIndex = returnType.LastIndexOf(">");
                    var type = returnType.Substring(startIndex + 1, endIndex - startIndex - 1);
                    returnType = returnType.Replace($"<{type}>", $"{type}");
                }
                if (method.ReturnType.ToString() == "void")
                {
                    source = "public async Task<Result> Placeholder;";
                }
                else
                {
                    source = $"public async Task<Result<{returnType}>> Placeholder;";
                }
            }
            else
            {
                if (method.ReturnType.ToString() == "void")
                {
                    source = "public Result Placeholder;";
                }
                else
                {
                    source = $"public Result<{method.ReturnType}> Placeholder;";
                }
            }
            
            var testTree = CSharpSyntaxTree.ParseText(source);
                
            var property = (FieldDeclarationSyntax)testTree.GetRoot().ChildNodes().First();
            identifier = property.Declaration.Type;
            return identifier;
        }
    }
}