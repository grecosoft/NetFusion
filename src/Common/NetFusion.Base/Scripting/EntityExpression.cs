using NetFusion.Common;
using System;

namespace NetFusion.Base.Scripting
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
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or contain an empty string", 
                    nameof(expression));
            }

            if (sequence < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence), 
                    "Expression sequence must be greater or equal to 0");
            }
            
            Expression = expression;
            Sequence = sequence;
            AttributeName = attributeName;
        }

        /// <summary>
        /// The optional name of the dynamic entity attribute associated with the expression.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// The expression script containing a short C# expression having full access to the 
        /// static domain entity properties and its set of dynamic runtime attribute values.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// The order in which the expression should be executed within the script.
        /// </summary>
        public int Sequence { get; }

        /// <summary>
        /// A description of the expression.
        /// </summary>
        public string Description { get; set; }
    }
}
