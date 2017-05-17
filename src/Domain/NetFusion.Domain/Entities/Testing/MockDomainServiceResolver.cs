using NetFusion.Domain.Entities.Registration;

namespace NetFusion.Domain.Entities.Testing
{
    /// <summary>
    /// Null domain service resolver.
    /// </summary>
    public class MockDomainServiceResolver : IDomainServiceResolver
    {
        public void ResolveDomainServices(IDomainBehavior domainBehavior)
        {
           
        }
    }
}
