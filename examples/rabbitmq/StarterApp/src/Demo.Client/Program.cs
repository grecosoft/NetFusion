using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.RabbitMQ.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Serialization;

namespace Demo.Client
{
    class Program
    {
        static void Main()
        {


            var serviceCollection = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();
            var configuration = configBuilder.AddAppSettings().Build();
            

            var loggerFactory = new LoggerFactory();
         
            loggerFactory.AddConsole(LogLevel.Warning);

            var appContainer = CreateAppContainer(serviceCollection, configuration, loggerFactory);
            appContainer.Start();

        }

        private static IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Demo.Client",
                "Demo.*");

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {
                    c.WithConfig((RabbitMqLoggerConfig config) => {
                        config.SetLogFactory(loggerFactory);
                    });

                    c.WithServices(reg => {
                        reg.AddSingleton<ISerializationManager, SerializationManager>();
                     });
                })
                .Build();
        }
    }
}
