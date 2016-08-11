using System;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Attribute that can be attached to classes or methods for specifying an associated script
    /// representing a predicate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ApplyScriptPredicateAttribute : Attribute
    {
        /// <summary>
        /// The name of the script stored external to the application.
        /// </summary>
        public string ScriptName { get; }

        /// <summary>
        /// Attribute Constructor
        /// </summary>
        /// <param name="scriptName">The name of the script stored external to the application.</param>
        public ApplyScriptPredicateAttribute(string scriptName)
        {
            this.ScriptName = scriptName;
        }

        /// <summary>
        /// If the entity being evaluated is of type IAttributedEntity used
        /// to specify the name of the dynamic attribute containing the result
        /// of the boolean expression.
        /// </summary>
        public string PredicateAttributeName { get; set; }

        /// <summary>
        /// The name of the static entity property containing the result
        /// of the boolean expression.
        /// </summary>
        public string PredicatePropertyName { get; set; }

        public ScriptPredicate ToPredicate()
        {
            return new ScriptPredicate
            {
                ScriptName = this.ScriptName,
                PredicateAttributeName = this.PredicateAttributeName,
                PredicatePropertyName = this.PredicatePropertyName
            };
        }
        
    }
}
