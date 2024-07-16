namespace Tripledot.ResultsGenerator
{
    public readonly struct SyntaxReceivedData
    {
        public readonly string MetadataName;
        public readonly string ClassNamespace;
        public readonly string ClassName;
        public readonly string MethodName;

        public string CombinedName => $"{MetadataName}.{MethodName}";
        public SyntaxReceivedData(string metadataName, string classNamespace, string className, string methodName)
        {
            MetadataName = metadataName;
            ClassNamespace = classNamespace;
            ClassName = className;
            MethodName = methodName;
        }
    }
}