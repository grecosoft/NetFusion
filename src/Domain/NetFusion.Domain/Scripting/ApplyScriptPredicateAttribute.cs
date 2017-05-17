using NetFusion.Common;
using System;

namespace NetFusion.Domain.Scripting
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
            Check.NotNullOrWhiteSpace(scriptName, nameof(scriptName));
            Check.NotNullOrWhiteSpace(variableName, nameof(variableName));

            this.ScriptName = scriptName;
            this.AttributeName = variableName;
        }

        public ScriptPredicate ToPredicate()
        {
            return new ScriptPredicate {
                ScriptName = this.ScriptName,
                AttributeName = this.AttributeName };
        }
    }
}
