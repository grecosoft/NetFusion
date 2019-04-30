using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Redis.Plugin.Modules;
using NetFusion.Redis.Publisher;
using NetFusion.Settings.Plugin;

namespace NetFusion.Redis.Plugin
{
    public class RedisPlugin : PluginBase
    {
        public override string PluginId => "6A52A70C-719B-41CF-AEFC-7CDFB586627A";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Plugin containing Redis extensions and Pub/Sub messaging.";

        public RedisPlugin()
        {
            AddModule<ConnectionModule>();
            AddModule<RedisModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddRedis(this ICompositeContainerBuilder composite)
        {
            // Add dependent plugins:
            composite
                .AddSettings()
                .AddMessaging();
            
            // Add Redis plugin:
            composite.AddPlugin<RedisPlugin>();
            
            // Integrate with base messaging plugin:
            var dispatchConfig = composite.GetPluginConfig<MessageDispatchConfig>();
            dispatchConfig.AddMessagePublisher<RedisPublisher>();
            
            return composite;
        }
    }
}