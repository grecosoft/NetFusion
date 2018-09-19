using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Redis.Internal;

namespace NetFusion.Redis.Modules
{
    public class RedisModule : PluginModule
    {
        /// <summary>
        /// Module containing additional Redis component registrations.
        /// </summary>
        /// <param name="services"></param>
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IRedisService, RedisService>();
        }
    }
}