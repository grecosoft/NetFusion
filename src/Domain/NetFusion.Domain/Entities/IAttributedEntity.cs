using System.Collections.Generic;

namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Identifies an entity having a set of key/value attributes that
    /// can be accessed dynamically at runtime.
    /// </summary>
    public interface IAttributedEntity
    {
        /// <summary>
        /// The interface for maintaining the entity's dynamic attributes.
        /// </summary>
        IEntityAttributes Attributes { get; }

        /// <summary>
        /// A dictionary of the key value pairs associated with the entity.
        /// </summary>
        IDictionary<string, object> AttributeValues { get; set; }
    }
}
