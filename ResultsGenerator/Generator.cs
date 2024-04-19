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
            var debug = new List<string>();

            try
            {
                foreach (var generatorData in syntaxReceiver.DataTypesToGenerate)
                {
                    debug.Add($"0 - {generatorData.MetadataName}");
                    var typeSymbol = context.Compilation.GetTypeByMetadataName(generatorData.MetadataName);
                    if (typeSymbol is null || errorsToGenerate.ContainsKey(generatorData.CombinedName)) continue;
                    
                    debug.Add("1");
                    
                    var members = typeSymbol.GetMembers();
                    var errors = new List<(int errorCode, string errorMessage)>();
                    
                    debug.Add("2");
                    
                    foreach (var member in members)
                    {
                        debug.Add("3");
                        
                        if (string.CompareOrdinal(member.Name, generatorData.MethodName) == 0)
                        {
                            debug.Add("4");
                            
                            var attributes = member.GetAttributes();
                            foreach (var attribute in attributes)
                            {
                                debug.Add("5");
                                if (AttributeUtils.IsErrorResultAttribute(attribute))
                                {
                                    debug.Add("6");
                                    
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
                    debug.Add("7");
                    errorsToGenerate.Add(generatorData.CombinedName, new ErrorResultData(generatorData,errors));
                }
            }
            finally
            {
                if (errorsToGenerate.Count > 0)
                {
                    var resultFactoryBuilder = new StringBuilder();
                    foreach (var result in errorsToGenerate)
                    {
                        var errorResultData = result.Value;
                        var debugResultsBuilder = new StringBuilder();
                        var releaseResultsBuilder = new StringBuilder();
                        foreach (var error in errorResultData.Errors)
                        {
                            var pascalCaseErrorMessage = StringTools.ToPascalCase(error.errorMessage);
                            var debugProperty = $@"
                            public Result {pascalCaseErrorMessage}
                                => new()
                                {{
                                    Succeeded = false, ErrorCode = {error.errorCode},
                                    ErrorMessage = ""{error.errorMessage}""
                                }};";
                            var releaseProperty = $@"
                            public Result {pascalCaseErrorMessage}
                                => new()
                                {{
                                    Succeeded = false,
                                    ErrorCode = {error.errorCode},
                                }};";
                            debugResultsBuilder.AppendLine(debugProperty);
                            releaseResultsBuilder.AppendLine(releaseProperty);
                        }
                        var resultsClass = $@"
                        using TDS.Results;

                        namespace {errorResultData.ClassNamespace}
                        {{
                            #if DEBUG
                            public static partial class ResultsFactory
                            {{
                                public static DnDApiResultsFactory DnDApi => new();
                            }}

                            public partial class DnDApiResultsFactory
                            {{
                                public AttackTheEnemyErrors AttackTheEnemy => new();
                            }}
                            
                            public class AttackTheEnemyErrors
                            {{
                                {debugResultsBuilder.ToString()}
                            }}
                        #else
                            public static class ResultsFactory
                            {{
                                public static DnDApiResultsFactory DnDApi => new();
                            }}

                            public class DnDApiResultsFactory
                            {{
                                public AttackTheEnemyErrors AttackTheEnemy => new();
                            }}

                            public class AttackTheEnemyErrors
                            {{
                                {releaseResultsBuilder.ToString()}
                            }}
                        #endif
                        }}
                        ";
                        FormattedFileWriter.WriteSourceFile(context: context,
                            sourceText: resultsClass,
                            fileName: $"Generated_{errorResultData.ClassNamespace}_{errorResultData.ClassName}");
                    }
                }
            }

            var stats = $@"
            public class OutputData
            {{
                public static string Data = ""Found {syntaxReceiver.DataTypesToGenerate.Count} methods with errors to generate"";
                public static string Methods = ""Methods inspected {syntaxReceiver.MethodsInspected.Count}. {String.Join(",", syntaxReceiver.MethodsInspected) }"";
                /* {String.Join(",", debug)} */
            }}
            ";
            
            FormattedFileWriter.WriteSourceFile(context: context,
                sourceText: stats,
                fileName: $"Generated_Stats");
            
            syntaxReceiver.DataTypesToGenerate.Clear();
        }

        
    }
}
