using InfrastructureTests.Web.Rest.Setup.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Modules;
using NetFusion.Test.Plugins;
using System.Collections.Generic;
using NetFusion.Rest.Client.Settings;
using Microsoft.Extensions.Logging;
using WebTests.Rest.Setup;

namespace InfrastructureTests.Web.Rest.Setup
{
    /// <summary>
    /// Extension methods used to create resource clients from a request settings configuration.
    /// 
    /// The extension methods creates a mock application host plug-in that will be discovered by the 
    /// NetFusion AppContainer when bootstrapped.  An in-memory web-host is created and integrates
    /// with the NetFusion AppContainer just as a real running web-host process.  
    /// 
    /// This allows complete integration testing of NetFusion plug-ins and a fully featured web-host. 
    /// While this is considered an integration test, they execute quickly since everything is executed
    /// in memory and no external resources such as databases are accessed.
    /// </summary>
    public static class TestClientExtensions
    {
        public static IRequestClient CreateTestClient(this IRequestSettings requestSettings,
            MockAppHostPlugin hostPlugin,
            IMockedService mockService = null)
        {
            requestSettings.UseHalDefaults();

            var serializers = new Dictionary<string, IMediaTypeSerializer>
            {
                { InternetMediaTypes.Json, new JsonMediaTypeSerializer() },
                { InternetMediaTypes.HalJson, new JsonMediaTypeSerializer() }
            };

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    // Add the needed ResourcePlugin modules since this is
                    // what the unit tests will be testing.
                    hostPlugin.UseResourcePlugin();

                    // Creates the typical StartUp class used by ASP.NET Core.
                    var startup = new TestStartup(hostPlugin);
                    services.AddSingleton<IStartup>(startup);

                    // This service will be injected by the WebApi controller and
                    // can be used to very the system under test.
                    services.AddSingleton<IMockedService>(mockService ?? new NullUnitTestService());
                }).UseSetting(WebHostDefaults.ApplicationKey, typeof(TestClientExtensions).Assembly.FullName);

           
            // Create an instance of the server and create an HTTP Client 
            // to communicate with in-memory web-host.
            var server = new TestServer(builder);
            var httpClient = server.CreateClient();
            var logger = new LoggerFactory().CreateLogger("Unit-Test Logger");

            // Return an instance of the ResourceClient to be tested.
            return new RequestClient(httpClient, logger, serializers, requestSettings);
        }
    }
}
