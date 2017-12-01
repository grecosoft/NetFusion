using NetFusion.Domain.Patterns.Queries.Config;
using NetFusion.Domain.Patterns.Queries.Filters;
using NetFusion.Domain.Patterns.Queries.Modules;
using NetFusion.Test.Plugins;
using System;

namespace DomainTests.Queries
{
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithDispatchConfiguredHost(this TestTypeResolver resolver, 
            params Type[] pluginTypes)
        {
            // Configure Core Plugin with messaging and the 
            // unit -of-work module.
            resolver.AddPlugin<MockCorePlugin>()
                 .AddPluginType<QueryDispatchModule>()
                 .AddPluginType<QueryFilterModule>()
                 .AddPluginType<QueryDispatchConfig>();

            // Add host plugin with the plugin-types to be used
            // for the unit-test.
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType(pluginTypes);

            return resolver;
        }
    }
}
