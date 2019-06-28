using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.MongoDB.Internal;
using NetFusion.MongoDB.Settings;

namespace NetFusion.MongoDB.Plugin.Modules
{
    /// <summary>
    /// Called by the base plug-in bootstrapping code.  Registers,
    /// any service component types associated with MongoDB that
    /// will be available as services at runtime.
    /// </summary>
    public class MongoModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            // NOTE:  Documentation states that the MongoClient class to which MongoDBClient
            // delegates is thread-safe and is best to register as single instance within
            // a dependency injection container.
            services.AddSingleton(typeof(IMongoDbClient<>), typeof(MongoDbClient<>));
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["database:settings"] = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<MongoSettings>())
                .Select(t => t.AssemblyQualifiedName);        
        }
    }
}
