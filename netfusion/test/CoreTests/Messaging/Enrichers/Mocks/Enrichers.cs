using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Types.Contracts;

namespace CoreTests.Messaging.Enrichers.Mocks
{
    public class MockEnricherWithException : MessageEnricher
    {
        public override Task EnrichAsync(IMessage message)
        {
            return Task.Run(() => throw new InvalidOperationException("TestEnricherException"));
        }
    }
}