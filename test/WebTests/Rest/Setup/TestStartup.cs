using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using NetFusion.Test.Plugins;
using System;

namespace InfrastructureTests.Web.Rest.Setup.Setup
{
    /// <summary>
    /// Start-Up class for ASP.NET Core used to configure the in-memory
    /// web-host that integrates with the NetFusion container.
    /// </summary>
    public class TestStartup : IStartup
    {
        private MockAppHostPlugin _pluginUnderTest;

        public TestStartup(MockAppHostPlugin pluginUnderTest)
        {
            _pluginUnderTest = pluginUnderTest;
        }

        public IAppContainer AppContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            
            // Add framework services.
            services.AddMvc(options =>
            {
                options.UseHalFormatter();
            });

            // Create, Build, and Start the NetFusion container.
            AppContainer = TestAppContainer.Create(_pluginUnderTest, services);

            AppContainer
                .Build()
                .Start();

            // Integrate the NetFusion container.
            return AppContainer.ServiceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
