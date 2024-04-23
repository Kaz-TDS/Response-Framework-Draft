using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using TDS.ResultsGenerator.Utils;

namespace TDS.ResultsGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver;
            if (syntaxReceiver is null) return;

            var errorsToGenerate = new Dictionary<string, ErrorResultData>();
            
            var generatedResultsFactories = new HashSet<string>();
            var generatedErrorsFactories = new HashSet<string>();
            var generatedErrorRepositories = new HashSet<string>();
            var generatedClassErrorProviders = new HashSet<string>();
            
            var debug = new StringBuilder();

             try
             {
                 foreach (var generatorData in syntaxReceiver.DataTypesToGenerate)
                 {
                     var typeSymbol = context.Compilation.GetTypeByMetadataName(generatorData.MetadataName);
                     if (typeSymbol is null || errorsToGenerate.ContainsKey(generatorData.CombinedName)) continue;

                     var members = typeSymbol.GetMembers();
                     var errors = new List<(int errorCode, string errorMessage)>();
                     IMethodSymbol method = null;

                     debug.AppendLine("");
                     debug.AppendLine($"CombinedName - {generatorData.CombinedName} - Members Count = {members.Length}");
                     foreach (var member in members)
                     {
                         debug.AppendLine($"MemberName - {member.Name}");
                         if (string.CompareOrdinal(member.Name, generatorData.MethodName) == 0)
                         {
                             debug.AppendLine($"{member.Name} Found");
                             method = member as IMethodSymbol;
                             var attributes = member.GetAttributes();
                             foreach (var attribute in attributes)
                             {
                                 debug.AppendLine($"Attribute {attribute.ToString()}");
                                 if (AttributeUtils.IsErrorResultAttribute(attribute))
                                 {
                                    var arguments = attribute.ConstructorArguments;
                                    if(arguments.Length == 2)
                                    { 
                                        errors.Add(
                                            (errorCode: int.Parse(arguments[0].Value.ToString()),
                                            errorMessage: arguments[1].Value.ToString()));
                                    }
                                 }
                             }
                         }
                     }

                     var returnType = method.ReturnType as INamedTypeSymbol;
                     var isGeneric = returnType.IsGenericType;
                     var returnValueType = String.Empty;
                     if (isGeneric)
                     {
                         returnValueType = GetFullTypeName(returnType.TypeArguments[0]);
                         debug.AppendLine($"Generic return - {returnValueType}");
                     }
                     
                     errorsToGenerate.Add(generatorData.CombinedName,
                         new ErrorResultData(generatorData,errors, isGeneric, returnValueType));
                 }
             }
             finally
             {
                if (errorsToGenerate.Count > 0)
                {
                    foreach (var result in errorsToGenerate)
                    {
                        var errorResultData = result.Value;
                        var errorCodeBuilder = new StringBuilder();
                        var debugResultsBuilder = new StringBuilder();
                        var releaseResultsBuilder = new StringBuilder();
                        foreach (var error in errorResultData.Errors)
                        {
                            var pascalCaseErrorMessage = StringTools.ToPascalCase(error.errorMessage);
                            debug.AppendLine($"Generating {pascalCaseErrorMessage} - is generic -> {errorResultData.IsGeneric}");
                            if (!errorResultData.IsGeneric)
                            {
                                var debugProperty = $@"
                               public Result {pascalCaseErrorMessage}
                                   => new()
                                   {{
                                       Succeeded = false, ErrorCode = {error.errorCode},
                                       ErrorMessage = ""{error.errorMessage}"",
                                   }};";
                                var releaseProperty = $@"
                               public Result {pascalCaseErrorMessage}
                                   => new()
                                   {{
                                       Succeeded = false,
                                       ErrorCode = {error.errorCode},
                                   }};";
                                var errorCodeProperty = $@"
                                public readonly int {pascalCaseErrorMessage}ErrorCode = {error.errorCode};
                                ";
                                errorCodeBuilder.AppendLine(errorCodeProperty);
                                debugResultsBuilder.AppendLine(debugProperty);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }
                            else
                            {
                                var genericType = errorResultData.ReturnTypeName;
                                var debugProperty = $@"
                               public Result<{genericType}> {pascalCaseErrorMessage}({genericType} response)
                                   => new()
                                   {{
                                       Succeeded = false, ErrorCode = {error.errorCode},
                                       ErrorMessage = ""{error.errorMessage}"",
                                       Response = response
                                   }};";
                                var releaseProperty = $@"
                               public Result<{genericType}> {pascalCaseErrorMessage}({genericType} response)
                                   => new()
                                   {{
                                       Succeeded = false,
                                       ErrorCode = {error.errorCode},
                                       Response = response
                                   }};";
                                var errorCodeProperty = $@"
                                public readonly int {pascalCaseErrorMessage}ErrorCode = {error.errorCode};
                                ";
                                errorCodeBuilder.AppendLine(errorCodeProperty);
                                debugResultsBuilder.AppendLine(debugProperty);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }
                        }

                        GenerateResultsFactory(errorResultData, ref generatedResultsFactories, context);
                        GenerateErrorRepository(errorResultData, ref generatedErrorRepositories, context);
                        GenerateErrorsFactory(errorResultData, ref generatedErrorsFactories, context);
                        GenerateClassErrorProvider(errorResultData, ref generatedClassErrorProviders, context);
                        GenerateErrors(errorResultData,
                            errorCodeBuilder,
                            debugResultsBuilder,
                            releaseResultsBuilder,
                            context);
                        GenerateMethodErrors(errorResultData, errorCodeBuilder, context);
                    }
                }
             }

            var stats = $@"
            public class OutputData
            {{
                 public static string Data = ""Found {syntaxReceiver.DataTypesToGenerate.Count} methods with errors to generate"";
                 public static string Methods = ""Methods inspected {syntaxReceiver.MethodsInspected.Count}. {String.Join(",", syntaxReceiver.MethodsInspected) }"";
/* 
{String.Join(",", debug)} 
*/
            }}
            ";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: stats,
                fileName: $"Generated_Stats");
            
            syntaxReceiver.DataTypesToGenerate.Clear();
        }

        private void GenerateMethodErrors(ErrorResultData data, StringBuilder errorCodes, GeneratorExecutionContext context)
        {
            var resultsClass = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
               public class {data.MethodName}ErrorCodes
               {{
                   {errorCodes}
               }}
            }}
            ";
            var fileName = $"Gen_{data.ClassNamespace}_{data.ClassName}_{data.MethodName}_ErrorCodes";
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsClass,
                fileName: fileName);
        }

        private void GenerateClassErrorProvider(ErrorResultData data, ref HashSet<string> generatedErrorProviders, GeneratorExecutionContext context)
        {
            if(generatedErrorProviders.Contains(data.ClassErrorsProviderUid)) return;
            
            var errorsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public partial class {data.ClassName}ErrorsProvider
                {{
                    public {data.MethodName}ErrorCodes {data.MethodName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: errorsFactory,
                fileName: $"Gen_{data.ClassErrorsProviderUid}");

            generatedErrorProviders.Add(data.ClassErrorsProviderUid);
        }

        private void GenerateErrorRepository(ErrorResultData data, ref HashSet<string> generatedRepositories, GeneratorExecutionContext context)
        {
            if(generatedRepositories.Contains(data.ErrorRepositoryUid)) return;
            
            var resultsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class ErrorRepository
                {{
                    public static {data.ClassName}ErrorsProvider {data.ClassName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsFactory,
                fileName: $"Gen_{data.ErrorRepositoryUid}");

            generatedRepositories.Add(data.ErrorRepositoryUid);
        }

        private void GenerateErrors(ErrorResultData data,
            StringBuilder errorCodes,
            StringBuilder debugErrors,
            StringBuilder releaseErrors,
            GeneratorExecutionContext context)
        {
            //TODO: Add handling for method overrides
            var resultsClass = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
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
            ";
            var fileName = $"Gen_{data.ClassNamespace}_{data.ClassName}_{data.MethodName}_Errors";
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsClass,
                fileName: fileName);
        }

        private void GenerateErrorsFactory(ErrorResultData data, ref HashSet<string> factories, GeneratorExecutionContext context)
        {
            if(factories.Contains(data.ErrorsFactoryUid)) return;
            
            var errorsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public partial class {data.ClassName}ResultsFactory
                {{
                    public {data.MethodName}Errors {data.MethodName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: errorsFactory,
                fileName: $"Gen_{data.ErrorsFactoryUid}");

            factories.Add(data.ErrorsFactoryUid);
        }

        private void GenerateResultsFactory(ErrorResultData data, ref HashSet<string> factories, GeneratorExecutionContext context)
        {
            if(factories.Contains(data.ResultsFactoryUid)) return;
            
            var resultsFactory = $@"
            using TDS.Results;

            namespace {data.ClassNamespace}
            {{
                public static partial class ResultsFactory
                {{
                    public static {data.ClassName}ResultsFactory {data.ClassName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsFactory,
                fileName: $"Gen_{data.ResultsFactoryUid}");

            factories.Add(data.ResultsFactoryUid);
        }

        private string GetFullTypeName(ITypeSymbol symbol)
        {
            var output = symbol.Name;
            if(symbol.ContainingType != null)
                output = $"{symbol.ContainingType.Name}.{output}";
            if (symbol.ContainingNamespace != null)
                output = $"{String.Join(" - ", symbol.ContainingNamespace.ConstituentNamespaces)}.{output}";
            return output;
        }
    }
}
