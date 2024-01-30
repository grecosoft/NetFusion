using NetFusion.Messaging.UnitTests.Messaging;

namespace NetFusion.Messaging.UnitTests.Commands.Mocks;

public class MockAsyncCommandConsumer(IMockTestLog testLog) : MockConsumer(testLog),
    IMessageConsumer
{
    [InProcessHandler]
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

    [InProcessHandler]
    public Task OnCommandNoResult(MockCommandNoResult command)
    {
        TestLog.AddLogEntry("Async-Command-Handler-No-Result");
        return Task.Run(() => { });
    }
}
    
public class MockSyncCommandConsumer(IMockTestLog testLog) : MockConsumer(testLog),
    IMessageConsumer
{
    [InProcessHandler]
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

public class MockInvalidCommandConsumer : MockConsumer,
    IMessageConsumer
{
    [InProcessHandler]
    public MockCommandResult InvalidHandler(MockCommand evt)
    {
        return new();
    }
}