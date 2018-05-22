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

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configBuilder) => SetupConfiguration(configBuilder))
                .UseStartup<Startup>()
                .Build();

        private static void SetupConfiguration(IConfigurationBuilder configBuilder)
        {
            configBuilder.AddAppSettings();
        }
    }
}
