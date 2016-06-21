﻿using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{

    public class MockAsyncMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public async Task OnEvent1(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent1));
            await Task.Run(() =>
            {

            });
        }

        [InProcessHandler]
        public async Task OnEvent2(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent2));
            await Task.Run(() =>
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