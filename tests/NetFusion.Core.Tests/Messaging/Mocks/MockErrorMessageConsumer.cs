using NetFusion.Messaging;
using System;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockErrorMessageConsumer : IMessageConsumer
    {
        public async Task OnEvent1(MockDomainEvent evt)
        {
            await Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent1));
            });
        }

        public async Task OnEvent2(MockDomainEvent evt)
        {
            await Task.Run(() =>
            {
                throw new InvalidOperationException(nameof(OnEvent2));
            });
        }
    }
}
