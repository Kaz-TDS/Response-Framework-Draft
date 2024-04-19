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

            try
            {
                foreach (var generatorData in syntaxReceiver.DataTypesToGenerate)
                {
                    var typeSymbol = context.Compilation.GetTypeByMetadataName(generatorData.ContainingTypeName);
                    if (typeSymbol is null || errorsToGenerate.ContainsKey(generatorData.CombinedName)) continue;
                    
                    var members = typeSymbol.GetMembers();
                    var errors = new List<(int errorCode, string errorMessage)>();
                    
                    foreach (var member in members)
                    {
                        if (string.CompareOrdinal(member.Name, generatorData.MethodName) == 0)
                        {
                            var attributes = member.GetAttributes();
                            foreach (var attribute in attributes)
                            {
                                if (AttributeUtils.IsErrorResultAttribute(attribute))
                                {
                                    var arguments = attribute.ConstructorArguments;
                                    if(arguments.Length == 2)
                                    {
                                        errors.Add(
                                        (FailureErrorCode: int.Parse(arguments[0].Value.ToString()),
                                            errorMessage: arguments[1].Value.ToString()));
                                    }
                                }
                            }
                        }
                    }
                    
                    errorsToGenerate.Add(generatorData.CombinedName, 
                        new ErrorResultData(namespaceName: generatorData.NamespaceName,
                            shortName: typeSymbol.Name, methodName: generatorData.MethodName,errors));
                }
            }
            finally
            {
                if (errorsToGenerate.Count > 0)
                {
                    var resultFactoryBuilder = new StringBuilder();
                    foreach (var result in errorsToGenerate)
                    {
                        foreach (var error in result.Value.Errors)
                        {

                        }
                    }

                    var generatorStats = $@"
using System;
using UnityEngine;

namespace Generated
{{
    [CreateAssetMenu(fileName = ""Data"", menuName = ""ScriptableObjects/RemoteConfigScriptableObject"", order = 1)]
    public class Generated_RemoteConfigDataHolder : ScriptableObject
    {{
{resultFactoryBuilder.ToString()}
    }}
}}
";
                    FormattedFileWriter.WriteSourceFile(context, generatorStats, $"Generated_RemoteConfigDataHolder");
                }
            }

            syntaxReceiver.DataTypesToGenerate.Clear();
        }

        
    }
}
