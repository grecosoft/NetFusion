namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

public abstract class MockQueryConsumer
{
    protected IMockTestLog TestLog { get; }

    protected MockQueryConsumer(IMockTestLog testLog)
    {
        TestLog = testLog;
    }
}
    
public class MockSyncQueryConsumer : MockQueryConsumer
{
    public MockSyncQueryConsumer(IMockTestLog testLog) : base(testLog) { }
        
    public MockQueryResult Execute(MockQuery query)
    {
        TestLog.AddLogEntry("Sync-Command-Handler");
        TestLog.RecordMessage(query);
            
        if (query.ThrowInHandlers.Contains(nameof(MockSyncQueryConsumer)))
        {
            throw new InvalidOperationException($"{nameof(MockSyncQueryConsumer)}_Exception");
        }
            
        return new MockQueryResult();
    }
}
    
public class MockAsyncQueryConsumer : MockQueryConsumer
{
    public MockAsyncQueryConsumer(IMockTestLog testLog) : base(testLog) { }
        
    public async Task<MockQueryResult> Execute(MockQuery query)
    {
        TestLog.AddLogEntry("Async-Command-Handler");

        await Task.Run(() =>
        {
            if (query.ThrowInHandlers.Contains(nameof(MockAsyncQueryConsumer)))
            {
                throw new InvalidOperationException($"{nameof(MockAsyncQueryConsumer)}_Exception");
            }
        });
            
        return new MockQueryResult();
    }
}
    
public class DuplicateConsumerOne 
{
    public MockQueryResult Execute(MockQuery query)
    {
        return new();
    }
}

public class DuplicateConsumerTwo 
{
    public MockQueryResult Execute(MockQuery query)
    {
        return new();
    }
}