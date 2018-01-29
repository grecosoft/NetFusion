using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
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
        // Discovered Properties:
        private IEnumerable<IEntityTypeMapping> EntityMappings { get; set; }

        private EntityFrameworkConfig Config { get; set; }

        public override void Initialize()
        {
            Config = Context.Plugin.GetConfig<EntityFrameworkConfig>();
        }

        // Factory delegate that sets the database context for the created
        // EntityContext<> closed type.  This allows the EntityContext<> to
        // delegate to the inner database context without inheritance.
        public override void RegisterDefaultComponents(ContainerBuilder builder)
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
            var contextMappings = Config.AutoRegisterTypeMappings ? GetContextMappings(contextType) : new IEntityTypeMapping[] { };

            var dbContext = Activator.CreateInstance(contextType, 
                contextSettings.ConnectionString, contextMappings);

            return (IEntityDbContext)dbContext;
        }

        private ContextConnection LookupContextConnection(IComponentContext componentContext, Type contextType)
        {
            var contextSettings = componentContext.ResolveOptional<ContextSettings>() ?? new ContextSettings();

            ContextConnection connection =  contextSettings.Connections?
                .FirstOrDefault(c => c.ContextName == contextType.Name);

            if (connection == null)
            {
                throw new InvalidOperationException(
                    $"Context connection could not be found for the following entity context type: {contextType}.");
            }
            return connection;
        }

        // Returns only the entity type mappings within the same or child namespace as the context.
        private IEntityTypeMapping[] GetContextMappings(Type contextType)
        {
            return EntityMappings.Where(map => 
                map.GetType().Namespace.StartsWith(contextType.Namespace, StringComparison.Ordinal))
                .ToArray();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["DB Contexts"] = Context.AllPluginTypes
                .Where(pt => pt.IsDerivedFrom<IEntityDbContext>())
                .Select(dc => dc.AssemblyQualifiedName)
                .ToArray();

            moduleLog["Entity Mappings"] = EntityMappings
                .Select(m => new
                {
                    MappingType = m.GetType().AssemblyQualifiedName
                }).ToArray();
        }
    }
}
