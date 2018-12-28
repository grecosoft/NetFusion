using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using NetFusion.Web.Mvc;
using System;
using NetFusion.Bootstrap.Configuration;

namespace TestClient.WebApi
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using TestClient.App.Services;
    using TestClient.Domain.Services;

    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        
        private readonly Autofac.ContainerBuilder _autofacBuilder = new Autofac.ContainerBuilder();

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)); 
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (EnvironmentConfig.IsDevelopment)
            {
                services.AddCors();                
            }

            // Support REST/HAL based API responses.
            services.AddMvc(options => {
                options.UseHalFormatter();
            });

            // Create and NetFusion application container based on Microsoft's abstractions:
            var builtContainer = CreateAppContainer(services, _configuration, _loggerFactory);

            // Start all modules in the application container and return created service-provider.
            // If an open-source DI container is needed, this is where it would be populated and returned.
            builtContainer.Start();
            
            var s = AppContainer.Instance.CreateServiceScope();
            var ts = s.ServiceProvider.GetService<ITestService>();
            var r = ts.GetValue();
            
            return builtContainer.ServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime applicationLifetime)
        {
            // This registers a method to be called when the Web Application is stopped.
            // In this case, we want to delegate to the NetFusion AppContainer so it can
            // safely stopped.
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseAuthentication();

            // Add additional middleware here.

            if (env.IsDevelopment())
            {
                string viewerUrl = _configuration.GetValue<string>("Netfusion:ViewerUrl");
                if (! string.IsNullOrWhiteSpace(viewerUrl))
                {
                    app.UseCors(builder => builder.WithOrigins(viewerUrl)
                        .AllowAnyMethod()
                        .WithExposedHeaders("WWW-Authenticate")
                        .AllowAnyHeader());
                }
            }
            
            app.UseMvc();
        }

        // Creates a NetFusion application container used to populate the service-collection 
        // for a set of discovered plug-ins.
        private IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "TestClient.WebApi",
                "TestClient.*");

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {

                    c.WithConfig((WebMvcConfig config) =>
                     {
                         config.EnableRouteMetadata = true;
                         config.UseServices(services);
                     })
                     .WithServices(ConfigureAutofac);
                })
                .Build();
        }
        
        private IServiceProvider ConfigureAutofac(IServiceCollection services)
        {
            _autofacBuilder.Populate(services);
            _autofacBuilder.RegisterType<TestService>()
                .As<ITestService>()
                .SingleInstance();
            
            var autoFactContainer = _autofacBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(autoFactContainer);

            return serviceProvider;
        }

        private static void OnShutdown()
        {
            AppContainer.Instance.Dispose();
        }
    }
}

