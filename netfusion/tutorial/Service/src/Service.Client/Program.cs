namespace Service.Client
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NetFusion.AMQP.Subscriber;
    using NetFusion.Base.Serialization;
    using NetFusion.Bootstrap.Configuration;
    using NetFusion.Bootstrap.Container;
    using NetFusion.Serialization;

    internal class Program
    {
       
        public static async Task Main(string[] args)
        {
            await BuildHost().WaitForShutdownAsync();
        }
        
        private static IHost BuildHost()
        {
            IBuiltContainer builtContainer = null;          
            
            var host = new HostBuilder()
                .ConfigureServices((context, collection) =>
                {
                    builtContainer = CreateAppContainer(collection, context.Configuration);
                })
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .Build();
            
            builtContainer.Start();
            host.Start();
            
            return host;
        }

        private static void SetupConfiguration(HostBuilderContext context, 
            IConfigurationBuilder builder)
        {            
            builder.AddAppSettings();
        }

        private static void SetupLogging(HostBuilderContext context, 
            ILoggingBuilder builder)
        {
            builder.ClearProviders();

            if (EnvironmentConfig.IsDevelopment)
            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
            }

            // Add additional logger specific to non-development environments.
        }
        
        private static IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Service.Client",
                "Service.*");
            
            var loggerFactory = new LoggerFactory();

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {

                    c.WithServices(reg =>
                    {
                        reg.AddSingleton<ISerializationManager, SerializationManager>();
                    
                        //  Additional services or overrides can be registered here.
                    });
                })
                .Build();
        }
    }
}
