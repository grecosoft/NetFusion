using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Config;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Strategies;
using System;

namespace RefArch.Subscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new Program();
            host.Bootstrap();

            Console.WriteLine("Client Started");
        }

        private void Bootstrap()
        {
            AppContainer.Create(new[] { "RefArch.*.exe, RefArch.*.dll" })
                .WithConfigSection("netFusion", "mongoAppSettings")

                .WithConfig((NetFusionConfig config) => {

                    config.AddSettingsInitializer(
                        typeof(FileSettingsInitializer<>));
                })

                .WithConfig((MessagingConfig config) =>
                {
                    // Only required if client will be publishing events.
                    //config.AddEventPublisherType<RabbitMqEventPublisher>();
                })

                .Build()
                .Start();
        }
    }
}
