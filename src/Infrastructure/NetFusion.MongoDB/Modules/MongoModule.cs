using Autofac;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.MongoDB.Configs;
using NetFusion.MongoDB.Core;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.MongoDB.Modules
{
    /// <summary>
    /// Called by the base plug-in bootstrapping code.  Registers,
    /// any service component types associated with MongoDB that
    /// will be available as services at runtime.
    /// </summary>
    public class MongoModule : PluginModule
    {
        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            // NOTE:  Documentation states that the class from which MongoDBClient
            // is thread-safe and is best to register a single instance within
            // a dependency injection container.
            builder.RegisterGeneric(typeof (MongoDbClient<>))
                .As(typeof (IMongoDbClient<>))
                .NotifyOnActivating()
                .SingleInstance();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Database-Settings"] = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<MongoSettings>())
                .Select(t => t.AssemblyQualifiedName);        
        }
    }
}
