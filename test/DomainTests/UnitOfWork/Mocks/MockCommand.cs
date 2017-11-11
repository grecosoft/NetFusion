using NetFusion.Messaging.Types;

namespace DomainTests.UnitOfWork.Mocks
{
    // Test command to invoke an action to test integration events.
    public class MockCommand : Command<MockAggregateOne>
    {
        public bool EnlistingSrvThrowEx { get; set; } = false;
        public bool IsCommittedAggregateInvalid { get; set; } = false;
        public bool IsEnlistedAggregateInvalid { get; set; } = false;
        public bool EnlistDuplicateIntegrationEvent { get; set; } = false;
    }
}
