using System.Diagnostics.CodeAnalysis;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Logging.Microsoft;
using EasyNetQ.Serialization.SystemTextJson;
using EasyNetQ.Topology;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin.Settings;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.RabbitMQ.Rpc;
using NetFusion.Integration.RabbitMQ.Rpc.Metadata;
using ISerializer = EasyNetQ.ISerializer;

namespace NetFusion.Integration.RabbitMQ.Bus;

/// <summary>
/// Implements a connection to the RabbitMQ broker by encapsulating all calls
/// to EasyNetQ.  This allows unit-testing the strategies easier since all
/// implementation details are container within this class and mocked.
/// </summary>
[ExcludeFromCodeCoverage]
public class BusConnection : IBusConnection
{
    private readonly IAdvancedBus _advancedBus;

    internal IBus Bus { get; }
    public ExternalEntitySettings ExternalSettings { get; }
    
    public BusConnection(
        ModuleContext moduleContext,  
        ConnectionSettings connectionSettings)
    {
        Bus = CreateBus(moduleContext, connectionSettings);
        ExternalSettings = new ExternalEntitySettings(connectionSettings);
        
        _advancedBus = Bus.Advanced;
    }
    
    private static IBus CreateBus(ModuleContext moduleContext, ConnectionSettings conn)
    {
        // Create an EasyNetQ connection configuration from application settings:
        ConnectionConfiguration connConfig = new ConnectionConfiguration {
            UserName = conn.UserName,
            Password = conn.Password,
            VirtualHost = conn.VHostName,
            RequestedHeartbeat = TimeSpan.FromSeconds(conn.Heartbeat),
            Name = conn.BusName,
            Product = moduleContext.AppHost.Name,
            Timeout =  TimeSpan.FromSeconds(conn.Timeout),
            PublisherConfirms = conn.PublisherConfirms,
            PersistentMessages = conn.PersistentMessages,
            PrefetchCount = conn.PrefetchCount,
            ConnectIntervalAttempt = conn.ConnectIntervalAttempt
        };

        SetAdditionalClientProperties(moduleContext, connConfig.ClientProperties);

        // Set associated hosts:
        connConfig.Hosts = conn.Hosts.Select(h => new HostConfiguration {
            Host = h.HostName,
            Port = h.Port
        }).ToArray();

        return RabbitHutch.CreateBus(connConfig, rs =>
        {
            rs.Register(typeof(IConsumerErrorStrategy), new ConsumerErrorStrategy());
            rs.Register(typeof(ISerializer), typeof(SystemTextJsonSerializer));
            rs.Register(typeof(EasyNetQ.Logging.ILogger<BusConnection>), typeof(MicrosoftLoggerAdapter<BusConnection>));
        });
    }
    
    private static void SetAdditionalClientProperties(ModuleContext moduleContext, 
        IDictionary<string, object> clientProps)
    {
        IPlugin rabbitPlugin = moduleContext.Plugin;
        IPlugin appHostPlugin = moduleContext.AppHost;
        
        clientProps["Client Assembly"] = rabbitPlugin.AssemblyName;
        clientProps["Client Version"] = rabbitPlugin.AssemblyVersion;
        clientProps["AppHost Name"] = appHostPlugin.Name;
        clientProps["AppHost Assembly"] = appHostPlugin.AssemblyName;
        clientProps["AppHost Version"] = appHostPlugin.AssemblyVersion;
        clientProps["AppHost Description"] = appHostPlugin.Description;
        clientProps["Machine Name"] = Environment.MachineName;          
    }

    // --- Exchange Creation ---
    
    public async Task<Exchange> CreateExchangeAsync(ExchangeMeta exchangeMeta)
    {
        return await _advancedBus.ExchangeDeclareAsync(exchangeMeta.ExchangeName, config =>
        {
            config.WithType(exchangeMeta.ExchangeType);
            config.AsDurable(exchangeMeta.IsDurable);
            config.AsAutoDelete(exchangeMeta.IsAutoDelete);

            if (! string.IsNullOrEmpty(exchangeMeta.AlternateExchangeName))
            {
                config.WithAlternateExchange(new Exchange(exchangeMeta.AlternateExchangeName));
            }
        });
    }
    
    public async Task BindQueueToExchange(string queueName, string exchangeName, string[] routeKeys)
    {
        var queue = new Queue(queueName);
        var exchange = new Exchange(exchangeName);
        
        foreach (string routeKey in routeKeys)
        {
            await _advancedBus.BindAsync(exchange, queue, routeKey);
        }
        
        if (routeKeys.Length == 0)
        {
            await _advancedBus.BindAsync(exchange, queue, string.Empty);
        }
    }
    
    // --- Queue Creation ---

    public async Task<Queue> CreateQueueAsync(QueueMeta queueMeta)
    {
        return await _advancedBus.QueueDeclareAsync(queueMeta.QueueName, config =>
        {
            config.AsExclusive(queueMeta.IsExclusive);
            config.AsDurable(queueMeta.IsDurable);
            config.AsAutoDelete(queueMeta.IsAutoDelete);
            
            if (queueMeta.MaxPriority != null)
            {
                config.WithMaxPriority(queueMeta.MaxPriority.Value);
            }

            if (queueMeta.PerQueueMessageTtl != null)
            {
                config.WithMessageTtl(TimeSpan.FromSeconds(queueMeta.PerQueueMessageTtl.Value));
            }

            if (! string.IsNullOrEmpty(queueMeta.DeadLetterExchangeName))
            {
                config.WithDeadLetterExchange(new Exchange(queueMeta.DeadLetterExchangeName));
            }
        });
    }
    
    // --- NetFusion Specific Exchanges ---
    
    public async Task CreateAlternateExchange(string exchangeName)
    {
        // Declare an exchange that will be sent all un-routed messages.
        var altExchange = await _advancedBus.ExchangeDeclareAsync(exchangeName, 
            config => config.WithType(ExchangeType.Fanout).AsDurable(true)
        );

        var undeliveredQueue = await _advancedBus.QueueDeclareAsync(exchangeName, config => config.AsDurable(true));
        await _advancedBus.BindAsync(altExchange, undeliveredQueue, string.Empty);
    }
    
    public async Task CreateDeadLetterExchange(string exchangeName)
    {
        // Declare an exchange that will be sent all un-routed messages.
        var altExchange = await _advancedBus.ExchangeDeclareAsync(exchangeName, 
            config => config.WithType(ExchangeType.Fanout).AsDurable(true)
        );

        var undeliveredQueue = await _advancedBus.QueueDeclareAsync(exchangeName, config => config.AsDurable(true));
        await _advancedBus.BindAsync(altExchange, undeliveredQueue, string.Empty);
    }
    
    // --- Message Publishing/Consuming
    public Task PublishToExchange(string exchangeName, string routeKey, bool isMandatory,
        MessageProperties messageProperties,
        byte[] messageBody,
        CancellationToken cancellationToken)
    { 
        return _advancedBus.PublishAsync(new Exchange(exchangeName),
            routeKey,
            isMandatory,
            messageProperties,
            messageBody, cancellationToken);
    }

    public Task PublishToQueue(string queueName, bool isMandatory,
        MessageProperties messageProperties,
        byte[] messageBody,
        CancellationToken cancellationToken = default)
    {
        return _advancedBus.PublishAsync(Exchange.Default,
            queueName,
            isMandatory,
            messageProperties,
            messageBody, cancellationToken);
    }

    public IDisposable ConsumeQueue(QueueMeta queueMeta,
        Func<byte[], MessageProperties, CancellationToken, Task> handler)
    {
        var queue = new Queue(queueMeta.QueueName);
        
        return _advancedBus.Consume(queue,
            (msgData, msgProps, _, cancellationToken) => 
                handler(msgData.ToArray(), msgProps, cancellationToken), 
            config =>
            {
                config.WithPrefetchCount(queueMeta.PrefetchCount);
                config.WithExclusive(queueMeta.IsExclusive);
                config.WithPriority(queueMeta.Priority);
            });
    }
    
    public IDisposable ConsumeRpcQueue(RpcQueueMeta queueMeta, 
        Func<byte[], MessageProperties, CancellationToken, Task> handler)
    {
        var queue = new Queue(queueMeta.QueueName);
        
        return _advancedBus.Consume(queue,
            (msgData, msgProps, _, cancellationToken) => 
                handler(msgData.ToArray(), msgProps, cancellationToken), 
            config =>
            {
                config.WithPrefetchCount(queueMeta.PrefetchCount);
                config.WithExclusive(false);
            });
    }

    public IDisposable ConsumeRpcReplyQueue(RpcReferenceEntity rpcEntity,
        Action<byte[], MessageProperties> handler)
    {
        var queue = new Queue(rpcEntity.ReplyQueueName);
        
        return _advancedBus.Consume(queue, (msgData, msgProps, _) =>
                handler(msgData.ToArray(), msgProps),
            config =>
            { 
                config.WithPrefetchCount(rpcEntity.PublishOptions.ResponseQueuePrefetchCount);
                config.WithExclusive();
            });
    }
}