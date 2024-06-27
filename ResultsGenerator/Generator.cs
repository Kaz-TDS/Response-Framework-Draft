using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
                    debug.AppendLine($"\nProcessing method - {generatorData.MethodName} - in type {generatorData.MetadataName}");
                    var typeSymbol = context.Compilation.GetTypeByMetadataName(generatorData.MetadataName);
                    if (typeSymbol is null || errorsToGenerate.ContainsKey(generatorData.CombinedName)) continue;

                    var members = typeSymbol.GetMembers();
                    var errors =
                        BuildErrorList(debug, generatorData, members, out var method);
                    
                    var returnType = (INamedTypeSymbol)method.ReturnType;
                    var isGenericReturnType = returnType.IsGenericType;
                    var isGenericMethod = method.TypeParameters.Length > 0;
                    var genericMethodParameters = new List<string>();
                    if (isGenericMethod)
                    {
                        foreach (var parameter in method.TypeParameters)
                        {
                            genericMethodParameters.Add(parameter.Name);
                        }

                        debug.AppendLine($"Generic method parameters - [{string.Join(",", genericMethodParameters)}]");
                    }
                    var returnValueType = String.Empty;
                    if(method.IsAsync)
                    {
                        var nestedType = (INamedTypeSymbol) returnType.TypeArguments[0];
                        if (nestedType.IsGenericType)
                        {
                            returnValueType = GetFullTypeName(nestedType.TypeArguments[0], genericMethodParameters);
                        }
                    }
                    else if (isGenericReturnType)
                    {
                        returnValueType = GetFullTypeName(returnType.TypeArguments[0], genericMethodParameters);
                        debug.AppendLine($"Generic return - {returnValueType}");
                    }

                    errorsToGenerate.Add(generatorData.CombinedName,
                        new ErrorResultData(generatorData, errors, isGenericReturnType, returnValueType, isGenericMethod, genericMethodParameters));
                }
            }
            catch (Exception e)
            {
                debug.AppendLine($"Caught an exception while processing methods - {e.Message}");
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
                        debug.AppendLine("Generating errors");
                        foreach (var error in errorResultData.Errors)
                        {
                            var pascalCaseErrorMessage = StringTools.ToPascalCase(error.errorMessage);
                            debug.AppendLine($"Generating {pascalCaseErrorMessage} ({error.errorCode}) - is generic -> {errorResultData.IsGenericReturnType}");
                            
                            if (!errorResultData.IsGenericReturnType)
                            {
                                var debugProperty = ErrorResultsUtils.GenerateSimpleDebugErrorResult(pascalCaseErrorMessage, error);
                                debugResultsBuilder.AppendLine(debugProperty);
                                
                                var releaseProperty = ErrorResultsUtils.GenerateSimpleReleaseErrorResult(pascalCaseErrorMessage, error);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }
                            else
                            {
                                var genericType = errorResultData.ReturnTypeName;
                                var debugProperty = ErrorResultsUtils.GenerateGenericDebugErrorResult(genericType, pascalCaseErrorMessage, result.Value.GenericParameters, error);
                                debugResultsBuilder.AppendLine(debugProperty);
                                
                                var releaseProperty = ErrorResultsUtils.GenerateGenericReleaseErrorResult(genericType, pascalCaseErrorMessage, result.Value.GenericParameters, error);
                                releaseResultsBuilder.AppendLine(releaseProperty);
                            }

                            debug.AppendLine("Generating the errorCode");
                                
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
                        ErrorCodeUtils.GenerateErrorRepository(errorResultData, ref generatedErrorRepositories, context);
                        ErrorCodeUtils.GenerateClassErrorProvider(errorResultData, ref generatedClassErrorProviders, context);
                        ErrorCodeUtils.GenerateMethodErrors(errorResultData, errorCodeBuilder, context);
                    }
                }
            }

            var debugData = $@"
            public class ResultsGenerator_DebugData
            {{
/* 
{String.Join(",", debug)} 
*/
            }}
            ";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: debugData,
                fileName: $"Generated_Stats");
            
            syntaxReceiver.DataTypesToGenerate.Clear();
        }

        private static List<(int errorCode, string errorMessage)> BuildErrorList(
            StringBuilder debug,
            SyntaxReceivedData generatorData,
            ImmutableArray<ISymbol> members,
            out IMethodSymbol method)
        {
            var errors = new List<(int errorCode, string errorMessage)>();
            method = null;

            debug.AppendLine("");
            debug.AppendLine($"CombinedName - {generatorData.CombinedName} - Members Count = {members.Length}");
            foreach (var member in members)
            {
                debug.AppendLine($"Member {Enum.GetName(typeof(SymbolKind), member.Kind)} - {member.Name}");
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

            return errors;
        }

        private static string GetFullTypeName(ITypeSymbol symbol, List<string> genericParameters = null)
        {
            var output = symbol.Name;
            if(symbol.ContainingType != null)
                output = $"{symbol.ContainingType.Name}.{output}";
            if (symbol.ContainingNamespace != null)
                output = $"{String.Join(" - ", symbol.ContainingNamespace.ConstituentNamespaces)}.{output}";
            var namedSymbol = symbol as INamedTypeSymbol;
            if (namedSymbol != null && namedSymbol.IsGenericType)
            {
                var genericTypes = new List<string>();
                foreach (var genericType in namedSymbol.TypeArguments)
                {
                    if (genericParameters != null && genericParameters.Contains(genericType.Name))
                    {
                        genericTypes.Add(genericType.Name);
                    }
                    else
                    {
                        genericTypes.Add(GetFullTypeName(genericType, genericParameters));
                    }
                }

                output = $"{output}<{String.Join(",",genericTypes)}>";
            }
            return output;
        }
    }
}
