using NetFusion.Messaging;
using System;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockErrorMessageConsumer : IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEvent1Async(MockDomainEvent evt)
        {
            return Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent1Async));
            });
        }

        [InProcessHandler]
        public Task OnEvent2Async(MockDomainEvent evt)
        {
            return Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent2Async));
            });
        }
    }
}
