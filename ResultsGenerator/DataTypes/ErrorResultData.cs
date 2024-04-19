using System.Collections.Generic;

namespace TDS.ResultsGenerator
{
    public class ErrorResultData
    {
        public string NamespaceName { get; private set; }
        public string ShortName { get; private set; }
        public string MethodName { get; private set; }
        public List<(int errorCode, string errorMessage)> Errors;

        public ErrorResultData(string namespaceName, string shortName, string methodName, List<(int errorCode, string errorMessage)> errors)
        {
            NamespaceName = namespaceName;
            ShortName = shortName;
            MethodName = methodName;
            Errors = errors;
        }
    }
}