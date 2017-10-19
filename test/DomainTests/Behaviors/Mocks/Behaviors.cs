using NetFusion.Domain.Entities;

namespace DomainTests.Behaviors.Mocks
{
    public interface IMockBehavior : IDomainBehavior
    {
        bool IsDefaultConstructorUsed { get; }
        Aggregates AssociatedAggregate { get; }
    }

    public class MockInvalidBehavior : IMockBehavior
    {
        public MockInvalidBehavior(Aggregates aggregate)
        {

        }

        public MockInvalidBehavior(AggregateTwo aggregate)
        {

        }

        public bool IsDefaultConstructorUsed => throw new System.NotImplementedException();

        public Aggregates AssociatedAggregate => throw new System.NotImplementedException();
    }

    public class MockBehaviorDefaultConstructor : IMockBehavior
    {
        public bool IsDefaultConstructorUsed { get; } = false;

        public Aggregates AssociatedAggregate => throw new System.NotImplementedException();

        public MockBehaviorDefaultConstructor()
        {
            IsDefaultConstructorUsed = true;
        }

        public MockBehaviorDefaultConstructor(int value)
        {

        }
    }

    public class MockBehaviorEntityConstructor : IMockBehavior
    {
        public Aggregates AssociatedAggregate { get; }

        public bool IsDefaultConstructorUsed => throw new System.NotImplementedException();

        public MockBehaviorEntityConstructor(Aggregates aggregate)
        {
            AssociatedAggregate = aggregate;
        }
    }
}
