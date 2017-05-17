using NetFusion.Common;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Represents a expression that can be executed at runtime against
    /// a given domain entity and/or its set of dynamic attributes.
    /// </summary>
    public class EntityExpression
    {
        public EntityExpression(
            string expression,
            int sequence,
            string attributeName = null)
        {
            Check.NotNullOrEmpty(expression, nameof(expression));
            Check.IsTrue(sequence >= 0, nameof(sequence),  "expression sequence must be greater or equal to 0");

            this.Expression = expression;
            this.Sequence = sequence;
            this.AttributeName = attributeName;
        }

        /// <summary>
        /// The name a dynamic entity attribute for the expression.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// The expression script containing a short C# expression having
        /// full access to the static domain entity and its set of dynamic
        /// runtime attribute values.
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
