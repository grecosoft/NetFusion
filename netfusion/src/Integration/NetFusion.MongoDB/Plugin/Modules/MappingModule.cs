﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.MongoDB.Internal;

namespace NetFusion.MongoDB.Plugin.Modules
{
    /// <summary>
    /// Called when the application is bootstrapped.  Finds all of the
    /// entity mapping classes and registers them with MongoDB.
    /// </summary>
    public class MappingModule : PluginModule, IMongoMappingModule
    {
        private static readonly object MapLock = new object();

        // Discovered Properties:
        public IEnumerable<IEntityClassMap> Mappings { get; private set; }

        // Configures MongoDB driver with mappings.
        public override void Configure()
        {
            lock(MapLock)
            {
                foreach (IEntityClassMap map in Mappings)
                {
                    if (! BsonClassMap.IsClassMapRegistered(map.EntityType))
                    {
                        map.AddKnownPluginTypes(Context.AllAppPluginTypes);
                        BsonClassMap.RegisterClassMap(map.ClassMap);
                    }
                }
            }  
        }

        public string GetEntityDiscriminator(Type mappedEntityType, Type knownEntityType)
        {
            if (mappedEntityType == null) throw new ArgumentNullException(nameof(mappedEntityType), 
                "Mapped entity type not specified.");

            if (knownEntityType == null) throw new ArgumentNullException(nameof(knownEntityType), 
                "Derived known type of mapped type not specified,");

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
                    $"The type of: {knownEntityType.FullName} is not a configured know type of: {mappedEntityType.FullName}");
            }

            var knowTypeMap = GetEntityMap(knownType);
            return knowTypeMap?.ClassMap.Discriminator ?? knownType.Name;
        }

        public IEntityClassMap GetEntityMap(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));

            return BsonClassMap.LookupClassMap(entityType) as IEntityClassMap;
        }

        public IEntityClassMap GetEntityMap<T>() where T : class
        {
            return GetEntityMap(typeof(T));
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["EntityMappings"] = Mappings.Select(m => new
            {
                MappingType = m.GetType().AssemblyQualifiedName,
                EntityType = m.EntityType.FullName,
                m.CollectionName,
                Descriminator = m.ClassMap.Discriminator,
                KnownTypes = m.ClassMap.KnownTypes.Select( kt => kt.FullName)
            }).ToArray();
        }
    }
}
