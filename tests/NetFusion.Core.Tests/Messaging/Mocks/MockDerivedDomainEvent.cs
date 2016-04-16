using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockBaseDomainEvent : DomainEvent
    {

    }

    public class MockDerivedDomainEvent : MockBaseDomainEvent
    {
    }
}
