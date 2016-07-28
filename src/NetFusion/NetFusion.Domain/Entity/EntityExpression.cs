namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Represents a dynamic expression script that can be
    /// executed at runtime against a given domain entity
    /// and/or its set of dynamic properties.
    /// </summary>
    public class EntityExpression
    {
        public EntityExpression(
            string expression,
            int sequence,
            bool isPersisted, 
            string propertyName = null)
        {
            this.Expression = expression;
            this.Sequence = sequence;
            this.IsPersisted = isPersisted;
            this.PropertyName = propertyName;
        }

        // The name of the defined property.
        public string PropertyName { get; }

        // The expression script containing a short C# expression/calculation
        // that has full access to the static domain entity and its set of 
        // dynamic runtime property values.
        public string Expression { get; }

        public int Sequence { get; }

        public bool IsPersisted { get; }


    }
}
