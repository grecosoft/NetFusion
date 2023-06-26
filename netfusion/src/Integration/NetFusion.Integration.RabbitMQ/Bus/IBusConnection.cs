using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.RabbitMQ.Rpc;
using NetFusion.Integration.RabbitMQ.Rpc.Metadata;

namespace NetFusion.Integration.RabbitMQ.Bus;

/// <summary>
/// Abstraction for connecting to the physical RabbitMQ broker.
/// </summary>
public interface IBusConnection
{
    /// <summary>
    /// Reference to external entity settings provided externally from the code.
    /// These setting override any corresponding settings specified within code.
    /// </summary>
    public ExternalEntitySettings ExternalSettings { get; }
    
    /// <summary>
    /// Creates an exchange based on the provided metadata.
    /// </summary>
    /// <param name="exchangeMeta">The metadata describing the exchange to be created.</param>
    /// <returns>Reference to the created exchange.</returns>
    Task<Exchange> CreateExchangeAsync(ExchangeMeta exchangeMeta);
    
    /// <summary>
    /// Creates an queue based on the provided metadata.
    /// </summary>
    /// <param name="queueMeta">The metadata describing the exchange to be created.</param>
    /// <returns>Reference to the created queue.</returns>
    Task<Queue> CreateQueueAsync(QueueMeta queueMeta);
    
    /// <summary>
    /// Binds a queue to an exchange.
    /// </summary>
    /// <param name="queueName">The name of the queue to be bound.</param>
    /// <param name="exchangeName">The name of the exchange to which the queue should be bound</param>
    /// <param name="routeKeys">The keys specifying which messages should be delivered to the bound queue.</param>
    /// <returns>Future result.</returns>
    Task BindQueueToExchange(string queueName, string exchangeName, string[] routeKeys);
    
    /// <summary>
    /// Creates an exchange and queue to receive any messages sent to an exchange for which there are
    /// no bound queues to which the message can be delivered.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange and queue for the alternate exchange.</param>
    /// <returns>Future result.</returns>
    Task CreateAlternateExchange(string exchangeName);
    
    /// <summary>
    /// Creates an exchange and queue to which dispatched messages, not processed by a
    /// consumer due to errors, are sent.
    /// </summary>
    /// <param name="exchangeName">the name of the exchange and queue for the dead letter exchange.</param>
    /// <returns></returns>
    Task CreateDeadLetterExchange(string exchangeName);

    /// <summary>
    /// Publishes a message to an exchange.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to which the message should be published.</param>
    /// <param name="routeKey">The key used to determine which exchange bindings a message should be delivered.</param>
    /// <param name="isMandatory">Determines what should happen if the message cannot be delivered.</param>
    /// <param name="messageProperties">Properties of the messages used for sending.</param>
    /// <param name="messageBody">The contents of the message.</param>
    /// <param name="cancellationToken">Cancellation token used to cancel the asynchronous task.</param>
    /// <returns>Future result.</returns>
    public Task PublishToExchange(string exchangeName, string routeKey, bool isMandatory,
        MessageProperties messageProperties,
        byte[] messageBody,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a message to a queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to which the message should be published.</param>
    /// <param name="isMandatory">Determines what should happen if the message cannot be delivered.</param>
    /// <param name="messageProperties">Properties of the messages used for sending.</param>
    /// <param name="messageBody">The contents of the message.</param>
    /// <param name="cancellationToken">Cancellation token used to cancel the asynchronous task.</param>
    /// <returns>Future result.</returns>
    Task PublishToQueue(string queueName, bool isMandatory,
        MessageProperties messageProperties,
        byte[] messageBody,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes a queue.
    /// </summary>
    /// <param name="queueMeta">The metadata describing the queue to be consumed.</param>
    /// <param name="handler">The handler invoked when message received.</param>
    /// <returns>Reference to object that can be disposed.</returns>
    IDisposable ConsumeQueue(QueueMeta queueMeta,
        Func<byte[], MessageProperties, CancellationToken, Task> handler);

    /// <summary>
    /// Consumes a RPC queue.
    /// </summary>
    /// <param name="queueMeta">The metadata determining the queue to be consumed.</param>
    /// <param name="handler">The handler invoked when message received.</param>
    /// <returns>Reference to object that can be disposed.</returns>
    IDisposable ConsumeRpcQueue(RpcQueueMeta queueMeta,
        Func<byte[], MessageProperties, CancellationToken, Task> handler);

    /// <summary>
    /// Consumes a RPC reply queue.
    /// </summary>
    /// <param name="rpcEntity">Entity describing RPC queue to be consumed.</param>
    /// <param name="handler">The handler invoked when message received.</param>
    /// <returns>Reference to object that can be disposed.</returns>
    IDisposable ConsumeRpcReplyQueue(RpcReferenceEntity rpcEntity,
        Action<byte[], MessageProperties> handler);
}