using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Integration.Redis.Subscriber;

namespace NetFusion.Integration.Redis.Plugin.Modules
{
    /// <summary>
    /// Module containing additional Redis component registrations.
    /// </summary>
    public class RedisModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<ISubscriptionService, SubscriptionService>();
        }
    }
}