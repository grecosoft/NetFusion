using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Composite.Core;

namespace NetFusion.Web.Mvc.Composite.Modules
{
    public class CompositeModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<CompositeService>()
                 .As<ICompositeService>()
                 .SingleInstance();
        }
    }
}
