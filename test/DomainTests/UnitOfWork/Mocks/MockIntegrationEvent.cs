using NetFusion.Messaging.Types;

namespace DomainTests.UnitOfWork.Mocks
{
    // Test event recorded by an aggregate used to integrate with
    // other application services.
    public class MockIntegrationEvent : DomainEvent
    {
        public bool EnlistingSrvThrowEx { get; set; } = false;
        public bool EnlistDuplicateIntegrationEvent { get; set; } = false;
        public bool IsEnlistedAggregateInvalid { get; set; } = false;
    }
}
