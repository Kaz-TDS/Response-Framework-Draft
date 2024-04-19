using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TDS.ResultsGenerator.Utils;

namespace TDS.ResultsGenerator
{
    public class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<GeneratorData> DataTypesToGenerate = new List<GeneratorData>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)) return;

            if (IsErrorResultDefinition(methodDeclarationSyntax))
            {
                var domainFullName = GetContainingTypeFullName(methodDeclarationSyntax);
                var namespaceName = GetNodeContainingNamespace(methodDeclarationSyntax);
                DataTypesToGenerate.Add(
                    new GeneratorData(
                        namespaceName: namespaceName,
                        containingTypeName: domainFullName,
                        methodName: methodDeclarationSyntax.Identifier.Text));
            }
        }

        private static bool IsErrorResultDefinition(SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodes()
                .OfType<AttributeSyntax>()
                .Any(AttributeUtils.IsErrorResultAttribute);
        }

        private static string GetNodeContainingNamespace(SyntaxNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode is NamespaceDeclarationSyntax namespaceNode)
                {
                    return namespaceNode.Name.ToString();
                }
                currentNode = currentNode.Parent;
            }
            return String.Empty;
        }

        private static string GetBaseTypeFullName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            if (baseTypeDeclarationSyntax.Parent is null) return baseTypeDeclarationSyntax.Identifier.Text;
            return $"{GetNodeFullName(baseTypeDeclarationSyntax.Parent)}.{baseTypeDeclarationSyntax.Identifier.Text}";
        }
        
        private static string GetContainingTypeFullName(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            if (memberDeclarationSyntax.Parent is BaseTypeDeclarationSyntax baseType)
            {
                return GetBaseTypeFullName(baseType);
            }
            return String.Empty;
        }

        private static string GetNodeFullName(SyntaxNode node)
        {
            var name = "";
            if (node.Parent != null)
            {
                var parentName = GetNodeFullName(node.Parent);
                name = string.IsNullOrEmpty(parentName) ? name : name + ".";
            }
            if (node is ClassDeclarationSyntax classNode)
            {
                name += classNode.Identifier.Text;
            }

            if (node is NamespaceDeclarationSyntax namespaceNode)
            {
                name += namespaceNode.Name;
            }

            return name;
        }
    }
}