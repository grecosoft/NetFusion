using Autofac;
using NetFusion.Domain.Entities.Registration;
using System;

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
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Resolves and injectable domain services specified on the behavior.
        /// </summary>
        /// <param name="domainBehavior">The domain behavior to have domain services resolved.</param>
        public void ResolveDomainServices(IDomainBehavior domainBehavior)
        {
            if (domainBehavior == null)throw new ArgumentNullException(nameof(domainBehavior));

            _container.InjectProperties(domainBehavior);
        }
    }
}
