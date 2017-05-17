using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Registration;

namespace DomainTests
{
    public class MockResolver : IDomainServiceResolver
    {
        public int ResolveCount { get; private set; }

        public void ResolveDomainServices(IDomainBehavior domainBehavior)
        {
            this.ResolveCount += 1;
        }
    }
}
