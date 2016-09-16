using NetFusion.Common;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Properties describing a script to be executed and the value of an entity
    /// attribute or property containing the result of the Boolean expression.
    /// </summary>
    public class ScriptPredicate
    {
        /// <summary>
        /// The name of the script that should be executed against the message to determine
        /// if it should be published to the exchange.
        /// </summary>
        public string ScriptName { get; }

        /// <summary>
        /// The entity attribute's Boolean property that determines if the message
        /// matches the criteria required to be published to the exchange.
        /// </summary>
        public string AttributeName { get; }

        public ScriptPredicate(string scriptName, string attributeName)
        {
            Check.NotNullOrWhiteSpace(scriptName, nameof(scriptName));
            Check.NotNullOrWhiteSpace(attributeName, nameof(attributeName));

            this.ScriptName = scriptName;
            this.AttributeName = attributeName;
        }
    }
}
