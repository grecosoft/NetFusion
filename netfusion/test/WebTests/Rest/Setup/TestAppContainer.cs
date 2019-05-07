using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;

namespace WebTests.Rest.Setup
{
    /// <summary>
    /// Creates and configures the NetFusion AppContainer used 
    /// by the unit-tests.
    /// </summary>
    public static class TestAppContainer
    {
        public static CompositeContainer Create(MockHostPlugin hostPlugin, IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().Build();
            var loggerFactory = new LoggerFactory();

            services.AddOptions();
            services.AddLogging();

            var container = new CompositeContainer(
                services,
                configuration,
                loggerFactory,
                setGlobalReference: false);
            
            container.RegisterPlugins(hostPlugin);

            return container;
        }
    }
}
