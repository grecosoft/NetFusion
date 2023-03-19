namespace NetFusion.Messaging.UnitTests.Commands.Mocks;

public class MockAsyncCommandConsumer : MockConsumer
{
    public MockAsyncCommandConsumer(IMockTestLog testLog) : base(testLog) { }
        
    public async Task<MockCommandResult> OnCommand(MockCommand command)
    {
        TestLog.AddLogEntry("Async-Command-Handler");

        var mockResponse = new MockCommandResult();

        await Task.Run(() => {
            mockResponse.Value = "MOCK_ASYNC_VALUE";
                
            if (command.ThrowInHandlers.Contains(nameof(MockAsyncCommandConsumer)))
            {
                throw new InvalidOperationException($"{nameof(MockAsyncCommandConsumer)}_Exception");
            }
        });

        return mockResponse;
    }
        
    public Task OnCommandNoResult(MockCommandNoResult command)
    {
        TestLog.AddLogEntry("Async-Command-Handler-No-Result");
        return Task.Run(() => { });
    }
}
    
public class MockSyncCommandConsumer : MockConsumer
{
    public MockSyncCommandConsumer(IMockTestLog testLog) : base(testLog) { }
        
    public MockCommandResult OnCommand(MockCommand command)
    {
        TestLog.AddLogEntry("Sync-Command-Handler");

        if (command.ThrowInHandlers.Contains(nameof(MockSyncCommandConsumer)))
        {
            throw new InvalidOperationException($"{nameof(MockSyncCommandConsumer)}_Exception");
        }
            
        return new MockCommandResult
        {
            Value = "MOCK_SYNC_VALUE"
        };
    }
}

public class MockInvalidCommandConsumer : MockConsumer
{
    public MockCommandResult InvalidHandler(MockCommand evt)
    {
        return new MockCommandResult();
    }
}