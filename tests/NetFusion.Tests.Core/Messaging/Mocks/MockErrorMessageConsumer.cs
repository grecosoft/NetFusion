using NetFusion.Messaging;
using System;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockErrorMessageConsumer : IMessageConsumer
    {
        [InProcessHandler]
        public async Task OnEvent1(MockDomainEvent evt)
        {
            await Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent1));
            });
        }

        [InProcessHandler]
        public async Task OnEvent2(MockDomainEvent evt)
        {
            await Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent2));
            });
        }
    }
}
