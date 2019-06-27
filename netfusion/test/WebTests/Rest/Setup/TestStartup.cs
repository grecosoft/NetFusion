using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using NetFusion.Rest.Server.Plugin.Configs;
using NetFusion.Rest.Server.Plugin.Modules;
using NetFusion.Test.Plugins;
using NetFusion.Web.Mvc.Plugin.Configs;
using NetFusion.Web.Mvc.Plugin.Modules;

namespace WebTests.Rest.Setup
{
    /// <summary>
    /// Start-Up class for ASP.NET Core used to configure the in-memory
    /// web-host that integrates with the NetFusion container.
    /// </summary>
    public class TestStartup : IStartup
    {
        private readonly MockHostPlugin _pluginUnderTest;

        public TestStartup(MockHostPlugin pluginUnderTest)
        {
            _pluginUnderTest = pluginUnderTest;
        }

        public CompositeContainer AppContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection serviceCollection)
        {
            
            // Add framework services.
            serviceCollection.AddMvc(options =>
            {
                options.UseHalFormatter();
            });

            // Create, Build, and Start the NetFusion container.
            AppContainer = TestAppContainer.Create(_pluginUnderTest, serviceCollection);
            
            var corePlugin = new MockCorePlugin();
            corePlugin.AddConfig<WebMvcConfig>();
            corePlugin.AddConfig<RestApiConfig>();
            corePlugin.AddModule<ApiMetadataModule>();
            corePlugin.AddModule<ResourceMediaModule>();
            corePlugin.AddModule<RestModule>();
            
            AppContainer.RegisterPlugins(corePlugin);

            var webMvcConfig = AppContainer.GetPluginConfig<WebMvcConfig>();
            webMvcConfig.EnableRouteMetadata = true;
            webMvcConfig.UseServices(serviceCollection);

            AppContainer.Compose(new TestTypeResolver());

            var services = AppContainer.AppBuilder.ServiceCollection.BuildServiceProvider();
            var compositeApp = services.GetService<ICompositeApp>();
            compositeApp.Start();

            return services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
