using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Services.Messaging.Enrichers;

/// <summary>
/// Adds the current date and time to the message if not already present.
/// </summary>
public class DateOccurredEnricher : IMessageEnricher
{
    // The MessageEnricherModule registers all enrichers with a scoped lifetime.  Therefore,
    // the below date value will be the same for all published messages during a given request.
    private readonly DateTime _scopedDateReceived = DateTime.UtcNow;
        
    public Task EnrichAsync(IMessage message)
    {
        message.SetUtcDateOccurred(_scopedDateReceived);
        return Task.CompletedTask;
    }
}