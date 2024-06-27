using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TDS.ResultsGenerator.Utils
{
    public static class ErrorResultsUtils
    {
        public static string GenerateGenericReleaseErrorResult(string genericReturnType, string pascalCaseErrorMessage, List<string> genericParameters,
            (int errorCode, string errorMessage) error)
        {
            var genericMethodParameters = genericParameters.Count == 0
                ? String.Empty
                : $"<{String.Join(",", genericParameters)}>";
            var releaseProperty = $@"
                                public Result<{genericReturnType}> {pascalCaseErrorMessage}{genericMethodParameters}({genericReturnType} response = default)
                                => new Result<{genericReturnType}>
                                (
                                    succeeded: false,
                                    errorCode: {error.errorCode},
                                    response: response
                                );";
            return releaseProperty;
        }

        public static string GenerateGenericDebugErrorResult(string genericReturnType, string pascalCaseErrorMessage, List<string> genericParameters,
            (int errorCode, string errorMessage) error)
        {
            var genericMethodParameters = genericParameters.Count == 0
                ? String.Empty
                : $"<{String.Join(",", genericParameters)}>";
            var debugProperty = $@"
                                public Result<{genericReturnType}> {pascalCaseErrorMessage}{genericMethodParameters}({genericReturnType} response = default)
                                => new Result<{genericReturnType}>
                                (
                                    succeeded: false,
                                    errorCode: {error.errorCode},
                                    errorMessage: ""{error.errorMessage}"",
                                    response: response
                                );";
            return debugProperty;
        }

        public static string GenerateSimpleReleaseErrorResult(string pascalCaseErrorMessage,
            (int errorCode, string errorMessage) error)
        {
            var releaseProperty = $@"
                                public Result {pascalCaseErrorMessage}
                                => new Result(succeeded: false, errorCode: {error.errorCode});";
            return releaseProperty;
        }

        public static string GenerateSimpleDebugErrorResult(string pascalCaseErrorMessage,
            (int errorCode, string errorMessage) error)
        {
            var debugProperty = $@"
                                public Result {pascalCaseErrorMessage}
                                => new Result(succeeded: false,
                                    errorCode: {error.errorCode},
                                    errorMessage: ""{error.errorMessage}""
                                );";
            return debugProperty;
        }

        public static void GenerateErrorResults(ErrorResultData data,
            StringBuilder debugErrors,
            StringBuilder releaseErrors,
            GeneratorExecutionContext context)
        {
            //TODO: Add handling for method overrides
            var resultsClass = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public partial class {data.ClassName}ResultsProvider
                {{
                   #if DEBUG
                   
                   public class {data.MethodName}Errors
                   {{
                       {debugErrors}
                   }}
                   #else

                   public class {data.MethodName}Errors
                   {{
                       {releaseErrors}
                   }}
                   #endif
                }}
            }}
            ";
            var fileName = $"Gen_{data.ClassNamespace}_{data.ClassName}_{data.MethodName}_Errors";
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsClass,
                fileName: fileName);
        }

        public static void GenerateErrorResultsProvider(ErrorResultData data, ref HashSet<string> factories, GeneratorExecutionContext context)
        {
            if(factories.Contains(data.ErrorsFactoryUid)) return;
            
            var errorsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public partial class {data.ClassName}ResultsProvider
                {{
                    public {data.MethodName}Errors {data.MethodName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: errorsFactory,
                fileName: $"Gen_{data.ErrorsFactoryUid}");

            factories.Add(data.ErrorsFactoryUid);
        }

        public static void GenerateResultsFactory(ErrorResultData data, ref HashSet<string> factories, GeneratorExecutionContext context)
        {
            if(factories.Contains(data.ResultsFactoryUid)) return;
            
            var resultsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class ResultsFactory
                {{
                    public static {data.ClassName}ResultsProvider {data.ClassName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsFactory,
                fileName: $"Gen_{data.ResultsFactoryUid}");

            factories.Add(data.ResultsFactoryUid);
        }
    }
}