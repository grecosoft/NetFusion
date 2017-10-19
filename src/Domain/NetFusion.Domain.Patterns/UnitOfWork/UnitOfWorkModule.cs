using Autofac;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// Module containing bootstrap registrations.
    /// </summary>
    public class UnitOfWorkModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<AggregateUnitOfWork>()
                .As<IAggregateUnitOfWork>()
                .InstancePerLifetimeScope();
        }
    }
}
