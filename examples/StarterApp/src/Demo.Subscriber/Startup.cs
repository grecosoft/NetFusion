using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Subscriber.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using NetFusion.RabbitMQ.Plugin;

namespace Demo.Subscriber
{
    public class Startup
    {
        // Microsoft Abstractions:
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, 
            ILogger<CompositeContainer> logger, 
            ILoggerFactory loggerFactory, 
            IHostingEnvironment hostingEnv)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _hostingEnv = hostingEnv;
           
        }
        
        public void ConfigureServices(IServiceCollection services)
        {        
            services.CompositeAppBuilder(_loggerFactory, _configuration)
                
                .AddRabbitMq()

                .AddPlugin<WebApiPlugin>()
                .Build();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseMvc();
        }
    }
}
