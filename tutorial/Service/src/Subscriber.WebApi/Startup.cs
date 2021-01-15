using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Hosting;
using NetFusion.Azure.ServiceBus.Plugin;
using NetFusion.Messaging.Plugin;
using NetFusion.Settings.Plugin;
using NetFusion.Builder;
using NetFusion.Messaging.Logging;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Serilog;
using Serilog;
using Subscriber.WebApi.Hubs;
using Subscriber.WebApi.Plugin;

namespace Subscriber.WebApi
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
                .AddSettings()
                .AddMessaging()
                .AddRabbitMq()
                .AddAzureServiceBus()
                // .AddAmqp()
                .AddRedis()

                .AddPlugin<WebApiPlugin>()
                .Compose();
            
            services.AddCors();
            services.AddControllers();
            services.AddHttpContextAccessor();

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
    }
}

