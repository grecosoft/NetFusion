namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Represents a expression that can be executed at runtime against
    /// a given domain entity and/or its set of dynamic properties.
    /// </summary>
    public class EntityExpression
    {
        public EntityExpression(
            string expression,
            int sequence,
            string propertyName = null)
        {
            this.Expression = expression;
            this.Sequence = sequence;
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// The name of the defined property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The expression script containing a short C# expression having
        /// full access to the static domain entity and its set of dynamic
        /// runtime property values.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// The order in which the expression should be executed within 
        /// the script.
        /// </summary>
        public int Sequence { get; }

        /// <summary>
        /// A description of the expression.
        /// </summary>
        public string Description { get; set; }
    }
}
