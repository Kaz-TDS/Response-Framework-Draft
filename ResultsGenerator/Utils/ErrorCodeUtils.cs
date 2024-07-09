using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TDS.ResultsGenerator.Utils
{
    public static class ErrorCodeUtils
    {
        public static void GenerateMethodErrors(ErrorResultData data, StringBuilder errorCodes, GeneratorExecutionContext context)
        {
            var resultsClass = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class ErrorCodeRepository
                {{
                    public static partial class {data.ClassName}Errors
                    {{
                       public static class {data.MethodName}
                       {{
                           {errorCodes}
                       }}
                    }}
                }}
            }}
            ";
            var fileName = $"Gen_{data.ClassNamespace}_{data.ClassName}_{data.MethodName}_ErrorCodes";
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsClass,
                fileName: fileName);
        }

        public static void GenerateClassErrorProvider(ErrorResultData data, ref HashSet<string> generatedErrorProviders, GeneratorExecutionContext context)
        {
            if(generatedErrorProviders.Contains(data.ClassErrorsProviderUid)) return;
            
            var errorsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class {data.ClassName}Errors
                {{}}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: errorsFactory,
                fileName: $"Gen_{data.ClassErrorsProviderUid}");

            generatedErrorProviders.Add(data.ClassErrorsProviderUid);
        }

        public static void GenerateErrorRepository(ErrorResultData data, ref HashSet<string> generatedRepositories, GeneratorExecutionContext context)
        {
            if(generatedRepositories.Contains(data.ErrorRepositoryUid)) return;
            
            var resultsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class ErrorCodeRepository
                {{
                    public static partial class {data.ClassName}Errors
                    {{}}
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsFactory,
                fileName: $"Gen_{data.ErrorRepositoryUid}");

            generatedRepositories.Add(data.ErrorRepositoryUid);
        }
    }
}