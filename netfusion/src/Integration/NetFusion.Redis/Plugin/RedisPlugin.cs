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
        public override string Name => "NetFusion:  Redis Plugin";

        public RedisPlugin()
        {
            AddModule<ConnectionModule>();
            AddModule<RedisModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();

            Description = "Plugin containing Redis database extensions and Pub/Sub messaging.";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        /// <summary>
        /// Adds a plugin to the composite container that can be used to communicate with
        /// a Redis database.  The plugin also allows subscribing domain-events to Redis
        /// channels.
        /// </summary>
        /// <param name="composite">Reference to the composite container builder.</param>
        /// <returns>Reference to the composite container builder.</returns>
        public static ICompositeContainerBuilder AddRedis(this ICompositeContainerBuilder composite)
        {
            return composite
                .AddSettings()
                .AddMessaging()
                .AddPlugin<RedisPlugin>()
                
                // Extends core messaging by added the Redis published to the pipeline.
                .InitPluginConfig<MessageDispatchConfig>(config => 
                    config.AddPublisher<RedisPublisher>());
        }
    }
}