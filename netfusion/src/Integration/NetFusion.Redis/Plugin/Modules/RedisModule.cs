using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Redis.Internal;

namespace NetFusion.Redis.Plugin.Modules
{
    /// <summary>
    /// Module containing additional Redis component registrations.
    /// </summary>
    public class RedisModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IRedisService, RedisService>();
        }
    }
}