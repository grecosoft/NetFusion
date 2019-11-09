using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;


namespace Demo.Subscriber
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.CompositeContainer(hostContext.Configuration)
                        .AddRabbitMq()
                        .AddRedis()
                        .AddPlugin<HostPlugin>()
                        .Compose();
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
                })
                .Build();
            
            var compositeApp = host.Services.GetRequiredService<ICompositeApp>();
            var lifetime = host.Services.GetRequiredService<IApplicationLifetime>();
            
            lifetime.ApplicationStopping.Register(() =>
            {
                compositeApp.Stop();
            });
            
            await compositeApp.StartAsync();
            await host.RunAsync();
        }
    }
}
