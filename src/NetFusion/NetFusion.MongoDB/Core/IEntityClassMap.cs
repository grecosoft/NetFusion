using MongoDB.Bson.Serialization;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.MongoDB.Core
{
    /// <summary>
    /// Identifies classes that are responsible for mapping data 
    /// entities to their corresponding MongoDb collection documents.
    /// </summary>
    public interface IEntityClassMap : IKnownPluginType
    {
        /// <summary>
        /// The type of the entity to which the mapping corresponds.
        /// </summary>
        /// <returns>The  type of the associated entity.</returns>
        Type EntityType { get; }

        /// <summary>
        /// The collection corresponding to the entity class type.
        /// </summary>
        /// <returns>Name of the collection.</returns>
        string CollectionName { get; set; }

        /// <summary>
        /// The MongoDB class map containing the details of the entity
        /// class mappings.
        /// </summary>
        /// <returns>Instance of class containing details of how the entity
        /// class maps to the underlying database collection.</returns>
        BsonClassMap ClassMap { get; }

        /// <summary>
        /// Adds a known type derived from the type associated with the 
        /// mappings.
        /// </summary>
        /// <param name="pluginTypes">List of derived types.</param>
        void AddKnownPluginTypes(IEnumerable<Type> pluginTypes);
    }
}
