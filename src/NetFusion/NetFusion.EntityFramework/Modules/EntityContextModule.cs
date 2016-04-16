﻿using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.EntityFramework.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.EntityFramework.Modules
{
    /// <summary>
    /// Plug-in module that registers the open-generic EntityContext type which delegates
    /// to the underlying Entity-Framework DBContext.  When an EntityContext closed-type 
    /// for a specific DBContext is injected, the underlying context is resolved and
    /// initialized.  The initialized instance is then injected into the EntityContext.
    /// 
    /// Also, the entity type mappings contained in the same or child namespace as the
    /// context are registered.
    /// </summary>
    public class EntityContextModule : PluginModule
    {
        public IEnumerable<IEntityTypeMapping> EntityMappings { get; private set; }

        private EntityFrameworkConfig Config { get; set; }

        public override void Initialize()
        {
            Config = Context.Plugin.GetConfig<EntityFrameworkConfig>();
        }

        // Factory delegate that sets the database context for the created
        // EntityContext<> closed type.  This allows the EntityContext<> to
        // delegate to the inner database context without inheritance.
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EntityContext<>))
                .InstancePerLifetimeScope()
                .OnActivating(handler =>
                {
                    var entityContext = handler.Instance as IEntityContext;
                    var dbContextType = entityContext.DbContextType;

                    var dbContext = CreateDbContext(handler.Context, dbContextType);
                    entityContext.SetDbContext(dbContext);

                }).As(typeof(IEntityContext<>));
        }

        // Creates and prepares the database context based on application settings.
        private IEntityDbContext CreateDbContext(IComponentContext componentContext, Type contextType)
        {
            var contextSettings = LookupContextConnection(componentContext, contextType);
            var contextMappings = LookupContextMappings(contextType);

            var dbContext = Activator.CreateInstance(contextType, 
                contextSettings.ConnectionString, contextMappings);

            return (IEntityDbContext)dbContext;
        }

        private ContextConnection LookupContextConnection(IComponentContext componentContext, Type contextType)
        {
            var contextSettings = componentContext.ResolveOptional<ContextSettings>() ?? new ContextSettings();
            var connection =  contextSettings.Connections?.FirstOrDefault(c => c.ContextName == contextType.Name);

            if (connection == null)
            {
                throw new InvalidOperationException(
                    $"context connection could not be found for the following entity context type: {contextType}");
            }
            return connection;
        }

        // TODO:  Check if there is away to see if mappings have been loaded yet...
        // There there is an OnActivating event...
        private IEnumerable<IEntityTypeMapping> LookupContextMappings(Type contextType)
        {
            if (Config.AutoRegisterTypeMappings)
            {
                return this.EntityMappings.Where(m => m.GetType().Namespace.StartsWith(contextType.Namespace));
            }

            return Enumerable.Empty<IEntityTypeMapping>();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Entity Mappings"] = this.EntityMappings
                .Select(m => new
                {
                    MappingType = m.GetType().AssemblyQualifiedName
                });
        }
    }
}
