using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TDS.ResultsGenerator.Utils
{
    public static class SyntaxUtils
    {
        public static bool IsErrorResultDefinition(SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodes()
                .OfType<AttributeSyntax>()
                .Any(AttributeUtils.IsErrorResultAttribute);
        }

        public static string GetNodeContainingNamespace(SyntaxNode node)
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

        public static string GetNodeContainingClassName(SyntaxNode node)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode is BaseTypeDeclarationSyntax baseTypeNode)
                {
                    return baseTypeNode.Identifier.Text;
                }

                currentNode = currentNode.Parent;
            }
            return String.Empty;
        }

        public static string GetBaseTypeFullName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            if (baseTypeDeclarationSyntax.Parent is null) return baseTypeDeclarationSyntax.Identifier.Text;
            return $"{GetNodeFullName(baseTypeDeclarationSyntax.Parent)}.{baseTypeDeclarationSyntax.Identifier.Text}";
        }

        public static string GetContainingTypeFullName(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            if (memberDeclarationSyntax.Parent is BaseTypeDeclarationSyntax baseType)
            {
                return GetBaseTypeFullName(baseType);
            }
            return String.Empty;
        }

        public static string GetNodeFullName(SyntaxNode node)
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