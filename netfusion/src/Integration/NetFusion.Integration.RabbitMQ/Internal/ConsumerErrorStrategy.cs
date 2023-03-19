using EasyNetQ.Consumer;

namespace NetFusion.Integration.RabbitMQ.Internal;

/// <summary>
/// The following will disable the default error strategy provided
/// by EasyNetQ allowing the RabbitMQ x-dead-letter-exchange queue
/// attribute to be used.
/// </summary>
public class ConsumerErrorStrategy : IConsumerErrorStrategy
{
    public void Dispose()
    {

    }

    public Task<AckStrategy> HandleConsumerErrorAsync(ConsumerExecutionContext context, Exception exception,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(AckStrategies.NackWithoutRequeue);
    }

    public Task<AckStrategy> HandleConsumerCancelledAsync(ConsumerExecutionContext context,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(AckStrategies.NackWithRequeue);
    }
}