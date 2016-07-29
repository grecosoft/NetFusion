namespace NetFusion.Integration.Domain.Evaluation
{
    /// <summary>
    /// Data model used to store a set of related domain entity expression.
    /// </summary>
    public class EntityExpressionMeta
    {
        public string PropertyName { get; set; }
        public string Expression { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }
    }
}

