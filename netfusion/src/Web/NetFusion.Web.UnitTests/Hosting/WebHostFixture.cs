using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Logging;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Extensions;
using NetFusion.Core.TestFixtures.Plugins;

namespace NetFusion.Web.UnitTests.Hosting;

/// <summary>
/// Class used to arrange a composite-container from which a composite-application
/// running within a test-server can be tested by issuing HTTP calls.
/// </summary>
public class WebHostFixture
{
    private Type _unitTestType;
        
    // Items on which the underlying host-builder uses for configuration 
    // on which the TestService is built.
    private IDictionary<string, string> _settings;
    private Action<IServiceCollection> _servicesConfig;
    private Action<IApplicationBuilder> _appServicesConfig;
        
    // The resulting service-provider created from the populated service-collection
    // used to create a lifetime scope for the current request under-test.
    private IServiceProvider _serviceProvider;
 
        
    /// <summary>
    /// Used to initialize and run an integration unit-test on a built in-memory web host.
    /// </summary>
    /// <param name="webHostTest">Delegate passed an instance of the WebHost that can be
    /// arranged and acted on.</param>
    /// <typeparam name="T">Reference to class contained within the unit-test from which
    /// controllers will be loaded.</typeparam>
    /// <returns></returns>
    public static Task TestAsync<T>(Func<WebHostFixture, Task> webHostTest)
    {
        ArgumentNullException.ThrowIfNull(webHostTest);

        var instance = new WebHostFixture
        {
            _unitTestType = typeof(T)
        };
        return webHostTest(instance);
    }

    private WebHostFixture() { }
        
    /// <summary>
    /// Application settings to be added to the WebHost on which the unit-test will be executed.
    /// </summary>
    /// <param name="settings">Dictionary of settings to be added.</param>
    /// <returns></returns>
    public WebHostFixture WithSettings(IDictionary<string, string> settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        return this;
    }

    /// <summary>
    /// Service to be added to the WebHost on which the unit-test will be executed.
    /// </summary>
    /// <param name="services">The service-collection to add services.</param>
    /// <returns></returns>
    public WebHostFixture WithServices(Action<IServiceCollection> services)
    {
        _servicesConfig = services ?? throw new ArgumentNullException(nameof(services));
        return this;
    }

    public WebHostFixture UsingAppSerivces(Action<IApplicationBuilder> appServices)
    {
        _appServicesConfig = appServices ?? throw new ArgumentNullException(nameof(appServices));
        return this;
    }
        
    /// <summary>
    /// Used to add the plugins form which the WebHost under test should be composed.
    /// </summary>
    /// <param name="compose"></param>
    /// <returns></returns>
    public WebServerConfig ComposedFrom(Action<ICompositeContainerBuilder> compose)
    {
        if (compose == null) throw new ArgumentNullException(nameof(compose));
            
        // Following is the typical code that is found within a WebApi project's Main method
        // of the Program class.
        var hostBuilder = new WebHostBuilder();

        hostBuilder
            .ConfigureServices((context, services) =>
            {
                // Create instance used to add plugins to the underlying service-collection.
                var compositeBuilder = services.CompositeContainer(context.Configuration, new TestTypeResolver(), 
                    new NullExtendedLogger());
                    
                // Allow the unit-test to add the need plugins and call the compose method
                // to populate the service-collection.
                compose(compositeBuilder);
                compositeBuilder.Compose();
                   
                // Allow the unit-test to register needed services.
                _servicesConfig?.Invoke(services);

                // Adds the MVC services to the service-collection and the Controllers from
                // the unit-test project.
                services.AddControllers().AddApplicationPart(_unitTestType.Assembly);
            })
            .ConfigureAppConfiguration(configBdr =>
            {
                configBdr.AddInMemoryCollection(_settings ?? new Dictionary<string, string>());
            })
            .Configure(builder =>
            {
                _serviceProvider = builder.ApplicationServices;
                builder.UseHttpsRedirection();
                builder.UseRouting();
                builder.UseAuthorization();
                builder.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

                _appServicesConfig?.Invoke(builder);

                // At this point, the ASP.NET has created a service-provider from the populated
                // service-collection.  Obtain and start the composite-application.
                var compositeApp = _serviceProvider.GetRequiredService<ICompositeApp>();
                compositeApp.Start();
                    
            })
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(WebHostFixture).Assembly.FullName);

        return new WebServerConfig(new TestServer(hostBuilder), () => _serviceProvider);
    }
}