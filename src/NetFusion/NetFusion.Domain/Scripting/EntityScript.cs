using NetFusion.Common;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Represents a set of ordered expressions that executed at runtime
    /// against a domain entity.
    /// </summary>
    public class EntityScript
    {
        /// <summary>
        /// Identity value for the script.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The type of the entity to which the script is associated.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// The expressions associated with the script.
        /// </summary>
        public IReadOnlyCollection<EntityExpression> Expressions { get; }

        public EntityScript()
        {
            this.ImportedAssemblies = new List<string>();
            this.ImportedNamespaces = new List<string>();
            this.InitialAttributes = new Dictionary<string, object>();
        }

        public EntityScript(
            string id,
            string name,
            string entityType,
            IReadOnlyCollection<EntityExpression> expressions) : this()
        {
            Check.NotNullOrWhiteSpace(id, nameof(id));
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNullOrWhiteSpace(entityType, nameof(entityType));

            this.Id = id;
            this.Name = name;
            this.EntityType = Type.GetType(entityType);

            this.Expressions = expressions;
 
        }

        /// <summary>
        /// The name identifying the script.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the script within the current application domain.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The dynamic entity attributes and their initial values that should be used
        /// if not set before the script is executed.
        /// </summary>
        public IDictionary<string, object> InitialAttributes { get; set; }

        /// <summary>
        /// The assembles that can be access by the script.
        /// </summary>
        public ICollection<string> ImportedAssemblies { get; set; }

        /// <summary>
        /// The name spaces that can be referenced by the script.
        /// </summary>
        public ICollection<string> ImportedNamespaces { get; set; }
    }
}
