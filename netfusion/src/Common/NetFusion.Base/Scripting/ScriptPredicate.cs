namespace NetFusion.Base.Scripting
{
    /// <summary>
    /// Properties describing a script to be executed and the value of an entity
    /// attribute containing the result of the Boolean expression.
    /// </summary>
    public class ScriptPredicate
    {
        /// <summary>
        /// The name of the script that should be executed against a source object
        /// to determine if it meets the criteria of the expression.
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// The entity's Boolean attribute that determines if the source object
        /// matches the criteria of the script.
        /// </summary>
        public string AttributeName { get; set; }
    }
}
