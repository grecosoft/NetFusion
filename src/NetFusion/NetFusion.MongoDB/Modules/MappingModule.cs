using Autofac;
using MongoDB.Bson.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
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
    internal class MappingModule : PluginModule, IMongoMappingModule
    {
        // IMongoMappingModule:
        public IEnumerable<IEntityClassMap> Mappings { get; private set; }

        // Configures MongoDB driver with mappings.
        public override void StartModule(ILifetimeScope scope)
        {
            var allPluginTypes = Context.GetPluginTypesFrom();

            // Allow each mapping to discover known types associated
            // with its entity mapping.
            this.Mappings.ForEach(m => m.AddKnownPluginTypes(allPluginTypes));

            this.Mappings.Select(m => m.ClassMap)
                .ForEach(BsonClassMap.RegisterClassMap);
        }

        // IMongoMappingModule:
        public string GetEntityDiscriminator(Type mappedEntityType, Type knownEntityType)
        {
            Check.NotNull(mappedEntityType, nameof(mappedEntityType), "mapped entity type not specified");
            Check.NotNull(knownEntityType, nameof(knownEntityType), "derived known type of mapped type not specified");

            var entityMapping = this.Mappings.FirstOrDefault(m => m.EntityType == mappedEntityType);
            if (entityMapping == null)
            {
                throw new InvalidOperationException(
                    $"Mapping is not registered for the class type: {mappedEntityType}.");
            }

            var derivedEntityMapping = entityMapping.ClassMap.KnownTypes
                .FirstOrDefault(kt => kt.UnderlyingSystemType == knownEntityType);

            if (derivedEntityMapping != null)
            {
                return derivedEntityMapping.Name;
            }
            
            return null;
        }

        // IMongoMappingModule:
        public IEntityClassMap GetEntityMap(Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType), "entity type not specified");

            return this.Mappings.FirstOrDefault(m => m.EntityType == entityType);
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
