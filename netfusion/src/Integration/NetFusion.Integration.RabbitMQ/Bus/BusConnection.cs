using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Logging.Microsoft;
using EasyNetQ.Serialization.SystemTextJson;
using EasyNetQ.Topology;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin.Settings;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using ISerializer = EasyNetQ.ISerializer;

namespace NetFusion.Integration.RabbitMQ.Bus;

public class BusConnection
{
    public ConnectionSettings ConnectionSettings { get; }

    public IBus Bus { get; }
    public IAdvancedBus AdvancedBus { get; }
    public ExternalEntitySettings ExternalSettings { get; }

    public BusConnection(
        ModuleContext moduleContext,  
        ConnectionSettings connectionSettings)
    {
        ConnectionSettings = connectionSettings;
        Bus = CreateBus(moduleContext, ConnectionSettings);
        AdvancedBus = Bus.Advanced;

        ExternalSettings = new ExternalEntitySettings(connectionSettings);
    }
    
    private IBus CreateBus(ModuleContext moduleContext, ConnectionSettings conn)
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

        return BusFactory(connConfig);
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
    
    /// <summary>
    /// Factory method used to return an IBus implementation.  Default to EasyNetQ but can also
    /// be used to provided a mock during unit testing.
    /// </summary>
    protected Func<ConnectionConfiguration, IBus> BusFactory { get; set; } = c => RabbitHutch.CreateBus(c, rs =>
    {
        rs.Register(typeof(IConsumerErrorStrategy), new ConsumerErrorStrategy());
        rs.Register(typeof(ISerializer), typeof(SystemTextJsonSerializer));
        rs.Register(typeof(EasyNetQ.Logging.ILogger<BusConnection>), typeof(MicrosoftLoggerAdapter<BusConnection>));
    });


    // --- Exchange Creation ---
    
    public async Task<Exchange> CreateExchangeAsync(ExchangeMeta exchangeMeta)
    {
        if (! string.IsNullOrEmpty(exchangeMeta.AlternateExchangeName))
        {
            await CreateAlternateExchange(exchangeMeta.AlternateExchangeName);
        }
        
        return await AdvancedBus.ExchangeDeclareAsync(exchangeMeta.ExchangeName, config =>
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
            await AdvancedBus.BindAsync(exchange, queue, routeKey);
        }
        
        if (! routeKeys.Any())
        {
            await AdvancedBus.BindAsync(exchange, queue, string.Empty);
        }
    }
    
    // --- Queue Creation ---

    public async Task<Queue> CreateQueueAsync(QueueMeta queueMeta)
    {
        if (! string.IsNullOrEmpty(queueMeta.DeadLetterExchangeName))
        {
            await CreateDeadLetterExchange(queueMeta.DeadLetterExchangeName);
        }
        
        return await AdvancedBus.QueueDeclareAsync(queueMeta.QueueName, config =>
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
    
    private async Task CreateAlternateExchange(string exchangeName)
    {
        // Declare an exchange that will be sent all un-routed messages.
        var altExchange = await AdvancedBus.ExchangeDeclareAsync(exchangeName, 
            config => config.WithType(ExchangeType.Fanout).AsDurable(true)
        );

        var undeliveredQueue = await AdvancedBus.QueueDeclareAsync(exchangeName, config => config.AsDurable(true));
        await AdvancedBus.BindAsync(altExchange, undeliveredQueue, string.Empty);
    }
    
    private async Task CreateDeadLetterExchange(string exchangeName)
    {
        // Declare an exchange that will be sent all un-routed messages.
        var altExchange = await AdvancedBus.ExchangeDeclareAsync(exchangeName, 
            config => config.WithType(ExchangeType.Fanout).AsDurable(true)
        );

        var undeliveredQueue = await AdvancedBus.QueueDeclareAsync(exchangeName, config => config.AsDurable(true));
        await AdvancedBus.BindAsync(altExchange, undeliveredQueue, string.Empty);
    }
}