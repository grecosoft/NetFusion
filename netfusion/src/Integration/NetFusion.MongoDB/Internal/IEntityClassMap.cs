using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using NetFusion.Base.Plugins;

namespace NetFusion.MongoDB.Internal
{
    /// <summary>
    /// Identifies classes that are responsible for mapping types 
    /// to their corresponding MongoDb collection.
    /// </summary>
    public interface IEntityClassMap : IKnownPluginType
    {
        /// <summary>
        /// The type to which the mapping corresponds.
        /// </summary>
        /// <returns>Mapped type.</returns>
        Type EntityType { get; }

        /// <summary>
        /// The name of the collection corresponding to the type.
        /// </summary>
        /// <returns>Name of the collection.</returns>
        string CollectionName { get; }

        /// <summary>
        /// The MongoDB class map containing the details of the type mappings.
        /// </summary>
        /// <returns>Instance of class containing details of how the type
        /// maps to the underlying database collection.</returns>
        BsonClassMap ClassMap { get; }

        /// <summary>
        /// Allows the mapping to add known mappings types.
        /// </summary>
        /// <param name="pluginTypes">List of all plug-in types.</param>
        void AddKnownPluginTypes(IEnumerable<Type> pluginTypes);
    }
}
