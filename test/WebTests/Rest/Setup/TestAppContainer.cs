using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;
using NetFusion.Web.Mvc;

namespace InfrastructureTests.Web.Rest.Setup
{
    /// <summary>
    /// Creates and configures the NetFusion AppContainer used 
    /// by the unit-tests.
    /// </summary>
    public static class TestAppContainer
    {
        public static IAppContainer Create(MockAppHostPlugin hostPlugin, IServiceCollection services)
        {
            var resolver = new TestTypeResolver();
            var configuration = new ConfigurationBuilder().Build();
            var loggerFactory = new LoggerFactory();

            resolver.AddPlugin(hostPlugin);
            services.AddOptions();
            services.AddLogging();

            var container = new AppContainer(
                services,
                configuration,
                loggerFactory,
                resolver, 
                setGlobalReference: false);

            container.WithConfig((WebMvcConfig config) =>
            {
                config.EnableRouteMetadata = true;
                config.UseServices(services);
            });

            return container;
        }
    }
}
