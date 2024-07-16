using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tripledot.ResultsGenerator.Utils
{
    public class AttributeUtils
    {
        #region Attribute Names

        public const string ErrorResultFullName = "ErrorResultAttribute";
        public static readonly string ErrorResultShortName = "ErrorResult";
        
        #endregion
        
        public static bool IsErrorResultAttribute(AttributeSyntax attributeSyntax)
        {
            return attributeSyntax.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Any(ins => ins.DescendantTokens()
                    .Any(st => st.Kind() == SyntaxKind.IdentifierToken
                               && (st.ValueText == ErrorResultFullName
                                   || st.ValueText == ErrorResultShortName)));
        }

        public static bool IsErrorResultAttribute(AttributeData attributeData)
        {
            return attributeData.AttributeClass.Name == ErrorResultFullName ||
                   attributeData.AttributeClass.Name == ErrorResultShortName;
        }
    }
}