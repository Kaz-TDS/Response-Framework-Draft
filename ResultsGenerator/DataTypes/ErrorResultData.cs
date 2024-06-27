using System;
using System.Collections.Generic;

namespace TDS.ResultsGenerator
{
    public class ErrorResultData
    {
        public readonly string ClassNamespace;
        public readonly string ClassName;
        public readonly string MethodName;
        public readonly List<(int errorCode, string errorMessage)> Errors;
        public readonly bool IsGenericMethod;
        public readonly List<string> GenericParameters;
        public readonly bool IsGenericReturnType;
        public readonly string ReturnTypeName;

        public string ResultsFactoryUid => $"{ClassNamespace}_{ClassName}";
        public string ErrorsFactoryUid => $"{ClassNamespace}_{ClassName}_{MethodName}";
        public string ErrorRepositoryUid => $"{ClassNamespace}_{ClassName}_ErrorRepository";
        public string ClassErrorsProviderUid => $"{ClassNamespace}_{ClassName}_{MethodName}_ErrorsProvider";

        public ErrorResultData(SyntaxReceivedData syntaxReceivedData,
            List<(int errorCode, string errorMessage)> errors,
            bool isGenericReturnType = false, string returnTypeName = default,
            bool isGenericMethod = false, List<string> genericParameters = null)
        {
            ClassNamespace = syntaxReceivedData.ClassNamespace;
            ClassName = syntaxReceivedData.ClassName;
            MethodName = syntaxReceivedData.MethodName;
            Errors = errors;
            IsGenericReturnType = isGenericReturnType;
            ReturnTypeName = returnTypeName;
            IsGenericMethod = isGenericMethod;
            GenericParameters = genericParameters;
        }
    }
}