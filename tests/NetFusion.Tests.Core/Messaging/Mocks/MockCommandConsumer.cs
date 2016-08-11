using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public async Task<MockCommandResult> OnCommand(MockCommand evt)
        {
            AddCalledHandler(nameof(OnCommand));

            var mockResponse = new MockCommandResult();

            await Task.Run(()=> {

                mockResponse.Value = "MOCK_VALUE";

            });

            return mockResponse;
        }
    }
}
