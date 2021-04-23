using System.Threading.Tasks;
using NetFusion.Messaging;

namespace CoreTests.Messaging.Commands.Mocks
{
    public class MockCommandConsumer : MockConsumer, IMessageConsumer
    {
        [InProcessHandler]
        public async Task<MockCommandResult> OnCommand(MockCommand evt)
        {
            AddCalledHandler(nameof(OnCommand));

            var mockResponse = new MockCommandResult();

            await Task.Run(() => {
                mockResponse.Value = "MOCK_VALUE";
            });

            return mockResponse;
        }

        [InProcessHandler]
        public Task OnCommandNoResult(MockCommandNoResult command)
        {
            AddCalledHandler(nameof(OnCommandNoResult));
            return Task.Run(() => { });
        }
    }

    public class MockInvalidCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void InvalidHandler1(MockCommand evt)
        {

        }
    }
}