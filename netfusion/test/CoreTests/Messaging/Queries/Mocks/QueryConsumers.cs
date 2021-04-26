using System;
using System.Threading.Tasks;
using NetFusion.Messaging;

namespace CoreTests.Messaging.Queries.Mocks
{
    public abstract class MockQueryConsumer
    {
        protected IMockTestLog TestLog { get; }
        
        protected MockQueryConsumer()
        {
        }

        protected MockQueryConsumer(IMockTestLog testLog)
        {
            TestLog = testLog;
        }
    }
    
    public class MockSyncQueryConsumer : MockQueryConsumer,
        IQueryConsumer
    {
        public MockSyncQueryConsumer(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public MockQueryResult Execute(MockQuery query)
        {
            TestLog.AddLogEntry(nameof(Execute));

            TestLog.RecordMessage(query);
            return new MockQueryResult();
        }
    }
    
    public class MockAsyncQueryConsumer : MockQueryConsumer,
        IQueryConsumer
    {
        public MockAsyncQueryConsumer(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public async Task<MockQueryResult> Execute(MockQuery query)
        {
            TestLog.AddLogEntry(nameof(Execute));

            await Task.Run(() =>
            {
                if (query.ThrowEx)
                {
                    throw new InvalidOperationException("TestQueryConsumerException");
                }
            });
            
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
