using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using NetFusion.Web.Mvc;
using NetFusion.Web.Mvc.Composite;
using System;
using NetFusion.Bootstrap.Configuration;

namespace Solution.Context.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
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

            var builtContainer = CreateAppContainer(services, _configuration, _loggerFactory);

            builtContainer.Start();
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

            if (env.IsDevelopment())
            {
                string viewerUrl = _configuration.GetValue<string>("Netfusion:ViewerUrl");
                if (! string.IsNullOrWhiteSpace(viewerUrl))
                {
                    app.UseCors(builder => builder.WithOrigins(viewerUrl)
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                }

                app.UseDeveloperExceptionPage();
                app.UseCompositeQuerying();
            }
        }

        // Creates a NetFusion application container used to populate the service-collection 
        // for a set of discovered plug-ins.
        private IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Solution.Context.WebApi",
                "Solution.Context.*");

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {

                    c.WithConfig((WebMvcConfig config) =>
                     {
                         config.EnableRouteMetadata = true;
                         config.UseServices(services);
                     })
                     .WithServices(reg =>
                     {
                         //  Any additional services or overrides can be registered here.
                     });
                })
                .Build();
        }

        private static void OnShutdown()
        {
            AppContainer.Instance.Stop();
        }
    }
}

