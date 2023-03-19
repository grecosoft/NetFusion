using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.MongoDB.Internal;
using NetFusion.Integration.MongoDB.Plugin.Settings;

namespace NetFusion.Integration.MongoDB.Plugin.Modules;

/// <summary>
/// Called by the base plug-in bootstrapping code.  Registers, any service component
/// types associated with MongoDB that will be available as services at runtime.
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
        moduleLog["DatabaseSettings"] = Context.AllPluginTypes
            .Where(t => t.IsConcreteTypeDerivedFrom<MongoSettings>())
            .Select(t => t.AssemblyQualifiedName);        
    }
}