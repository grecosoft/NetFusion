namespace NetFusion.Integration.Domain.Evaluation
{
    /// <summary>
    /// Data model used to store a set of related domain entity expression.
    /// </summary>
    public class EntityExpressionConfig
    {
        public string PropertyName { get; set; }
        public string Expression { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }
    }
}

// Assemblies and imports to reference.
// Repository to read roles from JSON file
// See how to have mongoDB driver user camelCase
