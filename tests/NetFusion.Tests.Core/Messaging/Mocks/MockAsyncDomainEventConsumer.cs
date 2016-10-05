using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{

    public class MockAsyncMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEvent1Async(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent1Async));
            return Task.Run(() =>
            {

            });
        }

        [InProcessHandler]
        public Task OnEvent2Async(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent2Async));
            return Task.Run(() =>
            {

            });
        }

        [InProcessHandler]
        public void OnEvent3(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent3));
        }
    }
}