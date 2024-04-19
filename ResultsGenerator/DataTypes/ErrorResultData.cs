using System.Collections.Generic;

namespace TDS.ResultsGenerator
{
    public class ErrorResultData
    {
        public readonly string ClassNamespace;
        public readonly string ClassName;
        public readonly string MethodName;
        public readonly List<(int errorCode, string errorMessage)> Errors;

        public ErrorResultData(GeneratorData generatorData, List<(int errorCode, string errorMessage)> errors)
        {
            ClassNamespace = generatorData.ClassNamespace;
            ClassName = generatorData.ClassName;
            MethodName = generatorData.MethodName;
            Errors = errors;
        }
    }
}