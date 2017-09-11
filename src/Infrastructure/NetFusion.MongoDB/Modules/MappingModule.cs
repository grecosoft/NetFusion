using MongoDB.Bson.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.MongoDB.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.MongoDB.Modules
{
    /// <summary>
    /// Called when the application is bootstrapped.  Finds all of the
    /// entity mapping classes and registers them with MongoDB.
    /// </summary>
    public class MappingModule : PluginModule, IMongoMappingModule
    {
        private static object _mapLock = new object();

        // IMongoMappingModule:
        public IEnumerable<IEntityClassMap> Mappings { get; private set; }

        // Configures MongoDB driver with mappings.
        public override void Configure()
        {
            lock(_mapLock)
            {
                foreach (IEntityClassMap map in this.Mappings)
                {
                    if (!BsonClassMap.IsClassMapRegistered(map.EntityType))
                    {
                        map.AddKnownPluginTypes(this.Context.AllPluginTypes);
                        BsonClassMap.RegisterClassMap(map.ClassMap);
                    }
                }
            }  
        }

        // IMongoMappingModule:
        public string GetEntityDiscriminator(Type mappedEntityType, Type knownEntityType)
        {
            Check.NotNull(mappedEntityType, nameof(mappedEntityType), "mapped entity type not specified");
            Check.NotNull(knownEntityType, nameof(knownEntityType), "derived known type of mapped type not specified");

            IEntityClassMap entityMapping = GetEntityMap(mappedEntityType);
            if (entityMapping == null)
            {
                throw new InvalidOperationException(
                    $"Mapping is not registered for the class type: {mappedEntityType}.");
            }

            var knownType = entityMapping.ClassMap.KnownTypes
                .FirstOrDefault(kt => kt == knownEntityType);

            if (knownType == null)
            {
                throw new InvalidOperationException(
                    $"The type of: {knownType.FullName} is not a configured know type of: {mappedEntityType}");
            }

            var knowTypeMap = GetEntityMap(knownType);
            return knowTypeMap?.ClassMap.Discriminator ?? knownType.Name;
        }

        // IMongoMappingModule:
        public IEntityClassMap GetEntityMap(Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType), "entity type not specified");

            return BsonClassMap.LookupClassMap(entityType) as IEntityClassMap;
        }

        public IEntityClassMap GetEntityMap<T>() where T : class
        {
            return GetEntityMap(typeof(T));
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Entity Mappings"] = this.Mappings.Select(m => new
            {
                MappingType = m.GetType().AssemblyQualifiedName,
                EntityType = m.EntityType.AssemblyQualifiedName,
                CollectionName = m.CollectionName,
                Descriminator = m.ClassMap.Discriminator,
                KnownTypes = m.ClassMap.KnownTypes.Select( kt => kt.AssemblyQualifiedName)
            });
        }
    }
}
