using System.Threading.Tasks;
using NetFusion.Messaging;

namespace CoreTests.Messaging.Commands.Mocks
{
    public class MockAsyncCommandConsumer : MockConsumer, 
        IMessageConsumer
    {
        public MockAsyncCommandConsumer(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public async Task<MockCommandResult> OnCommand(MockCommand command)
        {
            TestLog.AddLogEntry("OnCommand");

            var mockResponse = new MockCommandResult();

            await Task.Run(() => {
                mockResponse.Value = "MOCK_VALUE";
            });

            return mockResponse;
        }

        [InProcessHandler]
        public Task OnCommandNoResult(MockCommandNoResult command)
        {
            TestLog.AddLogEntry("OnCommandNoResult");
            return Task.Run(() => { });
        }
    }
    
    public class MockSyncCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        public MockSyncCommandConsumer(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public MockCommandResult OnCommand(MockCommand command)
        {
            TestLog.AddLogEntry("OnCommand");
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
}