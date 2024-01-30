using NetFusion.Messaging.UnitTests.Messaging;

namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

public abstract class MockQueryConsumer(IMockTestLog testLog)
{
    protected IMockTestLog TestLog { get; } = testLog;
}
    
public class MockSyncQueryConsumer(IMockTestLog testLog) : MockQueryConsumer(testLog),
    IMessageConsumer
{
    [InProcessHandler]
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
    
public class MockAsyncQueryConsumer(IMockTestLog testLog) : MockQueryConsumer(testLog),
    IMessageConsumer
{
    [InProcessHandler]
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
    
public class DuplicateConsumerOne : IMessageConsumer
{
    [InProcessHandler]
    public MockQueryResult Execute(MockQuery query)
    {
        return new();
    }
}

public class DuplicateConsumerTwo : IMessageConsumer
{
    [InProcessHandler]
    public MockQueryResult Execute(MockQuery query)
    {
        return new();
    }
}