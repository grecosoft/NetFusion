using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Configuration;

namespace Demo.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) 
        {
            var configuration = CreateConfiguration(args);

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .UseStartup<Startup>()
                .Build();
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder
                .AddAppSettings()
                .AddCommandLine(args);

            return builder.Build();
        }
    }
}