namespace TDS.ResultsGenerator
{
    public readonly struct GeneratorData
    {
        public readonly string MetadataName;
        public readonly string ClassNamespace;
        public readonly string ClassName;
        public readonly string MethodName;

        public string CombinedName => $"{MetadataName}.{MethodName}";
        public GeneratorData(string metadataName, string classNamespace, string className, string methodName)
        {
            MetadataName = metadataName;
            ClassNamespace = classNamespace;
            ClassName = className;
            MethodName = methodName;
        }
    }
}