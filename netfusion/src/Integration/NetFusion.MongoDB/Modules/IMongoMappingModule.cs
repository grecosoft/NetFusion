using NetFusion.Bootstrap.Plugins;
using NetFusion.MongoDB.Core;
using System;
using System.Collections.Generic;

namespace NetFusion.MongoDB.Modules
{
    /// <summary>
    /// Services exposed by the MongoDB Mapping Module.
    /// </summary>
    public interface IMongoMappingModule : IPluginModuleService
    {
        /// <summary>
        /// List of all the entity class mappings.
        /// </summary>
        IEnumerable<IEntityClassMap> Mappings { get; }

        /// <summary>
        /// Returns the mappings for a given entity type.
        /// </summary>
        /// <param name="entityType">The type of entity.</param>
        /// <returns>Returns the entity map associated with the entity.  If the specified
        /// entity doesn't have a registered mappings, a null reference is returned.</returns>
        IEntityClassMap GetEntityMap(Type entityType);

        /// <summary>
        /// Returns the mapping for a given entity type.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>Returns the entity map associated with the entity.  If the specified entity 
        /// doesn't have a registered mappings, a null reference is returned.</returns>
        IEntityClassMap GetEntityMap<T>() where T : class;

        /// <summary>
        /// Given a mapped typed, returns the associated name used
        /// to identify the derived type in MongoDB.  This corresponds
        /// to the _t MongoDB property.
        /// </summary>
        /// <param name="mappedEntityType">The mapped type.</param>
        /// <param name="knownEntityType">The derived mapped type.</param>
        /// <returns>The discriminator name.  If a mapping can't be found,
        /// and InvalidOperationException is raised.  </returns>
        string GetEntityDiscriminator(Type mappedEntityType, Type knownEntityType);
    }
}
