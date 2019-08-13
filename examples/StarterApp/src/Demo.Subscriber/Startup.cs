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
using NetFusion.AMQP.Plugin;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;

namespace Demo.Subscriber
{
    public class Startup
    {
        // Microsoft Abstractions:
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {        
            services.CompositeContainer(_configuration)
                
                .AddRabbitMq()
                .AddRedis()
              //  .AddAmqp()

                .AddPlugin<WebApiPlugin>()
                .Compose();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseMvc();
        }
    }
}
