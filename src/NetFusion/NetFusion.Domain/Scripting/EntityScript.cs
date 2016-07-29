using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Represents a set of ordered expressions that are 
    /// executed against a domain entity.
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
        public IReadOnlyCollection<EntityExpression> Expressions { get; }

        public EntityScript(
            string Id,
            string name,
            string entityType,
            IReadOnlyCollection<EntityExpression> expressions) 
        {
            this.Id = Id;
            this.Name = name;
            this.EntityType = Type.GetType(entityType);
            this.Expressions = expressions;
        }

        /// <summary>
        /// The name identifying the script.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Explanation of the script within the current application domain.
        /// </summary>
        public string Description { get; set; }
    }
}
