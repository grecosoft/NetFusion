using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Config;
using NetFusion.Redis.Publisher;

namespace NetFusion.Redis.Plugin
{
    public class RedisPlugin : Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "6A52A70C-719B-41CF-AEFC-7CDFB586627A";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Plugin containing Redis extensions and Pub/Sub messaging.";

        public RedisPlugin()
        {
            
            // Modules:
            AddModule<ConnectionModule>();
            AddModule<RedisModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddRedis(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<RedisPlugin>();
            
            var dispatchConfig = composite.GetConfig<MessageDispatchConfig>();
            dispatchConfig.AddMessagePublisher<RedisPublisher>();
            
            return composite;
        }
    }
}