using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Entities.Registration;
using System.Collections.Generic;

namespace NetFusion.Domain.Modules
{
    /// <summary>
    /// Plug-In module executed during the bootstrap process that configures
    /// and instance of the DomainEntityFactory.
    /// </summary>
    public class EntityBehaviorModule : PluginModule
    {
        public IEnumerable<IBehaviorRegistry> Registries { get; private set; }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.Register(_ => DomainEntityFactory.Instance)
                .As<IDomainEntityFactory>()
                .SingleInstance();
        }

        public override void StartModule(IContainer container, ILifetimeScope scope)
        {
            var resolver = new AutofacDomainServiceRosolver(container);
            var factory = new DomainEntityFactory(resolver);

            // Add the application's entity behavior registrations to the Domain-Entity Factory
            // and set the factory singleton instance.
            foreach (var registry in Registries)
            {
                registry.Register(factory);
            }

            DomainEntityFactory.SetInstance(factory);
        }
    }
}
