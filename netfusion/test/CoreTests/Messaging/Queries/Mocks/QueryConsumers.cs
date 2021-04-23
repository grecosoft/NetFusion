using System.Collections.Generic;
using NetFusion.Messaging;
using NetFusion.Messaging.Types.Contracts;

namespace CoreTests.Messaging.Queries.Mocks
{
    public abstract class MockQueryConsumerBase
    {
        private readonly List<string> _executedHandlers = new ();
        private readonly List<IQuery> _receivedQueries = new();

        protected MockQueryConsumerBase()
        {
            ExecutedHandlers = _executedHandlers;
            ReceivedQueries = _receivedQueries;
        }

        public IReadOnlyCollection<string> ExecutedHandlers { get; }
        public IReadOnlyCollection<IQuery> ReceivedQueries { get; }

        protected void AddCalledHandler(string handlerName)
        {
            _executedHandlers.Add(handlerName);
        }

        protected void RecordReceivedQuery(IQuery message) => _receivedQueries.Add(message);
    }
    
    public class MockQueryConsumer : MockQueryConsumerBase,
        IQueryConsumer
    {
        [InProcessHandler]
        public MockQueryResult Execute(MockQuery query)
        {
            RecordReceivedQuery(query);
            AddCalledHandler(nameof(Execute));
            
            return new MockQueryResult();
        }
    }

    public class DuplicateConsumerOne : IQueryConsumer
    {
        [InProcessHandler]
        public MockQueryResult Execute(MockQuery query)
        {
            return new();
        }
    }

    public class DuplicateConsumerTwo : IQueryConsumer
    {
        [InProcessHandler]
        public MockQueryResult Execute(MockQuery query)
        {
            return new();
        }
    }
}
