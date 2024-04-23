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
                                var debugProperty = ErrorResultsUtils.GenerateSimpleDebugErrorResult(pascalCaseErrorMessage, error);
                                debugResultsBuilder.AppendLine(debugProperty);
                                
                                var releaseProperty = ErrorResultsUtils.GenerateSimpleReleaseErrorResult(pascalCaseErrorMessage, error);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }
                            else
                            {
                                var genericType = errorResultData.ReturnTypeName;
                                var debugProperty = ErrorResultsUtils.GenerateGenericDebugErrorResult(genericType, pascalCaseErrorMessage, error);
                                debugResultsBuilder.AppendLine(debugProperty);
                                
                                var releaseProperty = ErrorResultsUtils.GenerateGenericReleaseErrorResult(genericType, pascalCaseErrorMessage, error);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }
                            
                            var errorCodeProperty = $@"
                                public readonly int {pascalCaseErrorMessage} = {error.errorCode};
                                ";
                            errorCodeBuilder.AppendLine(errorCodeProperty);
                        }

                        ErrorResultsUtils.GenerateResultsFactory(errorResultData, ref generatedResultsFactories, context);
                        ErrorResultsUtils.GenerateErrorResultsProvider(errorResultData, ref generatedErrorsFactories, context);
                        ErrorResultsUtils.GenerateErrorResults(errorResultData,
                            debugResultsBuilder,
                            releaseResultsBuilder,
                            context);
                        GenerateErrorRepository(errorResultData, ref generatedErrorRepositories, context);
                        GenerateClassErrorProvider(errorResultData, ref generatedClassErrorProviders, context);
                        GenerateMethodErrors(errorResultData, errorCodeBuilder, context);
                    }
                }
             }

            var stats = $@"
            public class ResultsGenerator_DebugData
            {{
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
                public static partial class ErrorCodeRepository
                {{
                    public static {data.ClassName}ErrorsProvider {data.ClassName} => new();
                }}
            }}";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: resultsFactory,
                fileName: $"Gen_{data.ErrorRepositoryUid}");

            generatedRepositories.Add(data.ErrorRepositoryUid);
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
