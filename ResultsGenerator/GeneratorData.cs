namespace TDS.ResultsGenerator
{
    public readonly struct GeneratorData
    {
        public readonly string NamespaceName;
        public readonly string ContainingTypeName;
        public readonly string MethodName;

        public string CombinedName => $"{ContainingTypeName}.{MethodName}";
        public GeneratorData(string namespaceName, string containingTypeName, string methodName)
        {
            NamespaceName = namespaceName;
            ContainingTypeName = containingTypeName;
            MethodName = methodName;
        }
    }
}