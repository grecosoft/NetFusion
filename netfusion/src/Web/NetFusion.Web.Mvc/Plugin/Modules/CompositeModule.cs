using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Composite;
using NetFusion.Web.Mvc.Composite.Core;

namespace NetFusion.Web.Mvc.Plugin.Modules
{
    public class CompositeModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ICompositeService, CompositeService>();
        }
    }
}
