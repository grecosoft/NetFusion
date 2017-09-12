namespace NetFusion.Base.Scripting
{
    /// <summary>
    /// Properties describing a script to be executed and the value of an entity
    /// attribute containing the result of the Boolean expression.
    /// </summary>
    public class ScriptPredicate
    {
        /// <summary>
        /// The name of the script that should be executed against the message to determine
        /// if it should be published to the exchange.
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// The entity's Boolean attribute that determines if the entity
        /// matches the criteria of the script.
        /// </summary>
        public string AttributeName { get; set; }
    }
}
