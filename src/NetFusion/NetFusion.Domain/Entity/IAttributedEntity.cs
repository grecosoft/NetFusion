using System.Collections.Generic;

namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Represents a domain entity that can be attributed with
    /// a set of dynamic property values.
    /// </summary>
    public interface IAttributedEntity
    {
        /// <summary>
        /// Allows access to the dynamic attributes using C# syntax.
        /// </summary>
        dynamic Attributes { get; }

        /// <summary>
        /// Used to set a given attribute value for the entity.  If the
        /// attribute exists, the value will be overridden.</summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        void SetAttributeValue(string name, object value);

        /// <summary>
        /// Deletes an existing entity attribute.
        /// </summary>
        /// <param name="name">The name of the attribute to delete.</param>
        /// <returns></returns>
        bool DeleteAttribute(string name);

        bool ContainsAttribute(string name);

        T GetAttributeValue<T>(string name);

        T GetAttributeValueOrDefault<T>(string name, T defaultValue = default(T));

        /// <summary>
        /// List of all associated dynamic properties.
        /// </summary>
        IEnumerable<EntityAttributeValue> AttributeValues { get; }
    }
}
