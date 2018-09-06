using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.MongoDB.Core;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.MongoDB.Settings;

namespace NetFusion.MongoDB.Modules
{
    /// <summary>
    /// Called by the base plug-in bootstrapping code.  Registers,
    /// any service component types associated with MongoDB that
    /// will be available as services at runtime.
    /// </summary>
    public class MongoModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            // NOTE:  Documentation states that the class from which MongoDBClient
            // is thread-safe and is best to register a single instance within
            // a dependency injection container.
            Context.AllPluginTypes.Where(t => t.IsConcreteTypeDerivedFrom<MongoSettings>())
                .ForEach(ms =>
                {
                    var implementationType = typeof(MongoDbClient<>).MakeGenericType(ms);
                    var serviceType = typeof(IMongoDbClient<>).MakeGenericType(ms);
                    services.AddSingleton(serviceType, implementationType);
                });
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["database:settings"] = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<MongoSettings>())
                .Select(t => t.AssemblyQualifiedName);        
        }
    }
}
