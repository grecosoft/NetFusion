using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.UnitTests.Enrichers.Mocks;

public class MockEnricherWithException : IMessageEnricher
{
    public Task EnrichAsync(IMessage message)
    {
        return Task.Run(() => throw new InvalidOperationException("TestEnricherException"));
    }
}