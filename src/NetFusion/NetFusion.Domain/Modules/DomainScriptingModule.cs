using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;

namespace NetFusion.Domain.Modules
{
    public class DomainScriptingModule : PluginModule
    {

        // Register the Null implementation of the entity scripting service to 
        // be used by default and can be specified by other plug-ins that implement
        // script evaluation.
        public override void RegisterDefaultComponents(Autofac.ContainerBuilder builder)
        {
            builder.RegisterType<NullEntityScriptingService>()
                .As<IEntityScriptingService>()
                .InstancePerLifetimeScope();
        }
    }
}
