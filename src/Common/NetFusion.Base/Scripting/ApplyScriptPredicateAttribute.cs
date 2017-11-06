using System;

namespace NetFusion.Base.Scripting
{
    /// <summary>
    /// Attribute that can be attached to classes or methods for specifying an
    /// associated script variable representing a predicate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ApplyScriptPredicateAttribute : Attribute
    {
        /// <summary>
        /// The name of the script stored external to the application.
        /// </summary>
        public string ScriptName { get; }

        /// <summary>
        /// The attribute name set by the script used to determine if a given entity satisfies the condition.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// Attribute Constructor
        /// </summary>
        /// <param name="scriptName">The name of the script stored external to the application.</param>
        public ApplyScriptPredicateAttribute(string scriptName, string variableName)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
                throw new ArgumentException("Script name cannot be null or empty string.", nameof(scriptName));

            if (string.IsNullOrWhiteSpace(variableName))
                throw new ArgumentException("Variable name cannot be null or empty string.", nameof(variableName));

            ScriptName = scriptName;
            AttributeName = variableName;
        }

        /// <summary>
        /// Returns a simple object containing information about the script predicate.
        /// </summary>
        /// <returns>Object instance describing script predicate.</returns>
        public ScriptPredicate ToPredicate()
        {
            return new ScriptPredicate {
                ScriptName = ScriptName,
                AttributeName = AttributeName };
        }
    }
}
