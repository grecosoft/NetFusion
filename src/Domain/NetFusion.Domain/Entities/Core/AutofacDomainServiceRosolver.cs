using Autofac;
using NetFusion.Common;
using NetFusion.Domain.Entities.Registration;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Implementation based on Autofac that will resolve properties on entity-behaviors
    /// corresponding to domain services.
    /// </summary>
    public class AutofacDomainServiceRosolver : IDomainServiceResolver
    {
        private IContainer _container;

        public AutofacDomainServiceRosolver(IContainer container)
        {
            Check.NotNull(container, nameof(container));
            _container = container;
        }

        /// <summary>
        /// Resolves and injectable domain services specified on the behavior.
        /// </summary>
        /// <param name="domainBehavior">The domain behavior to have domain services resolved.</param>
        public void ResolveDomainServices(IDomainBehavior domainBehavior)
        {
            Check.NotNull(domainBehavior, nameof(domainBehavior));
            _container.InjectProperties(domainBehavior);
        }
    }
}
