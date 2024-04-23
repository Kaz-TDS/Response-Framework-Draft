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
        public readonly bool IsGeneric;
        public readonly string ReturnTypeName;

        public string ResultsFactoryUid => $"{ClassNamespace}_{ClassName}";
        public string ErrorsFactoryUid => $"{ClassNamespace}_{ClassName}_{MethodName}";
        public string ErrorRepositoryUid => $"{ClassNamespace}_{ClassName}_ErrorRepository";
        public string ClassErrorsProviderUid => $"{ClassNamespace}_{ClassName}_{MethodName}_ErrorsProvider";

        public ErrorResultData(GeneratorData generatorData,
            List<(int errorCode, string errorMessage)> errors,
            bool isGeneric = false, string returnTypeName = default)
        {
            ClassNamespace = generatorData.ClassNamespace;
            ClassName = generatorData.ClassName;
            MethodName = generatorData.MethodName;
            Errors = errors;
            IsGeneric = isGeneric;
            ReturnTypeName = returnTypeName;
        }
    }
}