using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tripledot.ResultsGenerator.Utils;

namespace Tripledot.ResultsGenerator
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<SyntaxReceivedData> DataTypesToGenerate = new List<SyntaxReceivedData>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)) return;
            
            if (SyntaxUtils.IsErrorResultDefinition(methodDeclarationSyntax))
            {
                DataTypesToGenerate.Add(
                    new SyntaxReceivedData(
                        metadataName: SyntaxUtils.GetContainingTypeFullName(methodDeclarationSyntax),
                        classNamespace: SyntaxUtils.GetNodeContainingNamespace(methodDeclarationSyntax),
                        className: SyntaxUtils.GetNodeContainingClassName(methodDeclarationSyntax),
                        methodName: methodDeclarationSyntax.Identifier.Text));
            }
        }
    }
}