using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Hosting;
using NetFusion.Messaging.Plugin;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Settings.Plugin;
using Service.App.Plugin;
using Service.Domain.Plugin;
using Service.Infra.Plugin;
using Service.WebApi.Plugin;
using NetFusion.Builder;
using NetFusion.Messaging.Logging;
using NetFusion.MongoDB.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Rest.Client;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Serilog;
using Serilog;
using Service.App.Services;
using Service.WebApi.Hubs;

namespace Service.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnv;

        
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnv)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _hostingEnv = hostingEnv ?? throw new ArgumentNullException(nameof(hostingEnv));
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.CompositeContainer(_configuration, new SerilogExtendedLogger())
                // Add common plugins:
                .AddSettings()
                .AddMessaging()
                
                // Add technology specific plugins:
                .AddMongoDb()
                .AddRabbitMq()
                .AddRedis()
                //.AddAmqp()
                .AddRest()
                .AddRestDocs()
                
                // Add application centric plugins:
                .AddPlugin<DomainPlugin>()
                .AddPlugin<AppPlugin>()    
                .AddPlugin<InfraPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Compose();
            
            services.AddCors();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddSingleton(InMemoryScripting.LoadSensorScript());
            
            services.AddRestClientFactory();
            services.AddDefaultMediaSerializers();
            RegisterHttpClients(services);

            if (_hostingEnv.IsDevelopment())
            {
                services.AddSignalR();
                services.AddMessageLogSink<HubMessageLogSink>();
            }
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime, IWebHostEnvironment env)
        {
            string viewerUrl = _configuration.GetValue<string>("Netfusion:ViewerUrl");
            if (! string.IsNullOrWhiteSpace(viewerUrl))
            {
                app.UseCors(builder => builder.WithOrigins(viewerUrl)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate")
                    .AllowAnyHeader());
            }
            
            app.UseSerilogRequestLogging();

            app.UseRestDocs();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                if (env.IsDevelopment())
                {
                    endpoints.MapHub<MessageLogHub>("/api/message/log");
                }
            });
        }

        private static void RegisterHttpClients(IServiceCollection services)
        {
            services.AddHttpClient("test", c =>
            {
                c.BaseAddress = new Uri("http://localhost:6400");
                c.DefaultRequestHeaders.Add("Accept", "application/hal+json");
            });
        }
    }
}

