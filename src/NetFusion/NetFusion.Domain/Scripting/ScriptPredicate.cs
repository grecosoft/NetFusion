using NetFusion.Common.Extensions;
using System;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Properties describing a script to be executed and the value of an entity
    /// attribute or property containing the result of the boolean expression.
    /// </summary>
    public class ScriptPredicate
    {
        /// <summary>
        /// The name of the script that should be executed against the message to determine
        /// if it should be published to the exchange.
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// The entity attribute's boolean property that determines if the message
        /// matches the criteria required to be published to the exchange.
        /// </summary>
        public string PredicateAttributeName { get; set; }

        /// <summary>
        /// The entity's boolean property that determines if the message
        /// matches the criteria required to be published to the exchange.
        /// </summary>
        public string PredicatePropertyName { get; set; }

        /// <summary>
        /// Validates the state of the predicate.
        /// </summary>
        public void Validate()
        {
            if (this.ScriptName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException($"{nameof(ScriptName)} must be specified.");
            }

            if (this.PredicateAttributeName.IsNullOrWhiteSpace() 
                && this.PredicatePropertyName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    $"Either the {nameof(PredicateAttributeName)} or {nameof(PredicatePropertyName)} must be specified.");
            }
        }
    }
}
