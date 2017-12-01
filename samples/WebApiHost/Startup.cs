using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using NetFusion.Bootstrap.Container;
using NetFusion.Web.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using WebApiHost.Filters;

namespace WebApiHost
{
    /// <summary>
    /// Called when bootstrapping the web application.
    /// </summary>
    public class Startup
    {
        private IHostingEnvironment Environment { get; set; }

        /// <summary>
        /// Bootstraps NETFusion application container and other application
        /// services. 
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            this.Environment = env;
        }

        /// <summary>
        /// Bootstraps NETFusion application container and other application services. 
        /// The AutoFac container built by the NetFusion AppContainer is configured as
        /// the service provider.
        /// </summary>
        /// <param name="services">Reference used to register services.</param>
        /// <returns>Configured service provider to use for dependency injection.</returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(config => {
                config.Filters.Add(typeof(ExceptionHandlerFilter));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "ToDo API",
                    Description = "A simple example ASP.NET Core Web API",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Shayne Boyer", Email = "", Url = "https://twitter.com/spboyer" },
                    License = new License { Name = "Use under LICX", Url = "https://example.com/license" }
                });

                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "WebApiHost.xml");
                c.IncludeXmlComments(xmlPath);
            });

            AppContainerSetup.Bootstrap(services);
            return new AutofacServiceProvider(AppContainer.Instance.Services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();
           // app.UseRouteMetadata();
        }
    }
}
