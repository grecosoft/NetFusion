using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Plugin.Modules
{
    /// <summary>
    /// Plugin module encapsulating the concerns of the publisher.  Finds all exchange definitions
    /// describing the exchanges/queues to which messages can be sent.  This discovered metadata
    /// is then used to create the needed exchanges and queues by delegating to the EasyNetQ
    /// advanced API.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        // Dependent Modules:
        protected IBusModule BusModule { get; set; }

        // Other  plugins (normally application plugins) specify the exchanges 
        // to be created by defining one or more IExchangeRegistry derived types.
        public IEnumerable<IExchangeRegistry> Registries { get;  protected set; }  
        
        // Records, for a given message type, the exchange metadata and the exchange
        // created from the metadata to which the message should be published.
        private IDictionary<Type, ExchangeMeta> _messageExchanges;
        private readonly IDictionary<Type, CreatedExchange> _createdExchanges;
        
        // Stores for the unique set of named RPC exchanges the associated RPC client that will create a queue,
        // on the default exchange, to process replies from the consumer containing the response to the original
        // sent command.  A client and replay queue is created for each unique RPC exchange name allowing a single
        // queue to process replies from various RPC commands.  It also allows the application to group common
        // commands based on concern or processing distribution needs.
        private readonly IDictionary<string, IRpcClient> _exchangeRpcClients;

        public PublisherModule()
        {
            _createdExchanges = new Dictionary<Type, CreatedExchange>();
            _exchangeRpcClients = new Dictionary<string, IRpcClient>();
        }

        //----------- [Plugin Initialization]  ---------------
      
        public override void Configure()
        {
            var definitions = Registries.SelectMany(r => r.GetDefinitions()).ToArray();

            ApplyConfiguredOverrides(definitions);
            AssertExchangeDefinitions(definitions);

            _messageExchanges = definitions.ToDictionary(m => m.MessageType);
        } 
        
        // All exchange types:  Direct, Topic, and Fan-out are defined within code with the most common default
        // values.  These default values can be overridden by specifying them within the application's configuration.
        private void ApplyConfiguredOverrides(IEnumerable<ExchangeMeta> definitions)
        {
            foreach (ExchangeMeta definition in definitions)
            {
                BusModule.ApplyExchangeSettings(definition);
            }
        }
        
        private static void AssertExchangeDefinitions(ExchangeMeta[] definitions)
        {
            AssertNoDuplicateExchanges(definitions);
            AssertNoDuplicateQueues(definitions);
            AssertNoDuplicateMessageTypes(definitions);
        }

        private static void AssertNoDuplicateExchanges(ExchangeMeta[] definitions)
        {
            // All exchange names that are not RPC exchanges must be unique for given named bus.
            // Default exchanges are excluded since they don't have names.
            var duplicateExchangeNames = definitions.Where(
                    d => !d.IsDefaultExchange && !d.IsRpcExchange)
                .WhereDuplicated(d => new { d.BusName, d.ExchangeName});
            
            if (duplicateExchangeNames.Any())
            {
                throw new ContainerException(
                    "Exchange names must be unique for a given named bus.  The following have been configured " +
                    "more than once.", "duplicate-exchanges", duplicateExchangeNames);
            }
            
            // Validate that all RPC exchanges are unique by bus, queue, and action namespace.
            var duplicateRpcExchanges = definitions.Where(d => d.IsRpcExchange)
                .WhereDuplicated(d => new { d.BusName, d.QueueMeta.QueueName, d.ActionNamespace});
            
            if (duplicateRpcExchanges.Any())
            {
                throw new ContainerException(
                    "For RPC exchanges names must be unique for a given named bus, queue, and action namespace.  " +
                    "The following have been configured more than once.", 
                    "duplicate-queues", duplicateRpcExchanges);
            }
        }
        
        private static void AssertNoDuplicateQueues(IEnumerable<ExchangeMeta> definitions)
        {
            var duplicateQueueNames = definitions.Where(d => d.IsDefaultExchange && !d.IsRpcExchange)
                .WhereDuplicated(d => new { d.BusName, d.QueueMeta.QueueName });
            
            if (duplicateQueueNames.Any())
            {
                throw new ContainerException(
                    "Queue names, defined on the default exchange, must be unique for a given named bus.  "  +
                    "The following have been configured more than once.", 
                    "duplicate-queues", duplicateQueueNames);
            } 
        }

        private static void AssertNoDuplicateMessageTypes(IEnumerable<ExchangeMeta> definitions)
        {
            var duplicateMessages = definitions.GroupBy(d => d.MessageType)
                .Where(g => g.Count() > 1)
                .Select(g => new {
                    MessageType = g.Key,
                    Definitions = g.Select(d => $"Exchange: {d.ExchangeName}, Queue: {d.QueueMeta?.QueueName}")
                }).ToArray();

            if (duplicateMessages.Any())
            {
                throw new ContainerException(
                    "Message type can only be associated with one exchange or queue.", 
                    "duplicate-message-types", duplicateMessages);
            } 
        }

        
        //----------- [Plugin Execution] ---------------
        
        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {
            await CreateExchanges(_messageExchanges.Values);
            await CreateRpcExchangeClients(_messageExchanges.Values);

            BusModule.Reconnection += async (sender, args) => await HandleReconnection(args);
        }

        protected override Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach(IRpcClient client in _exchangeRpcClients.Values)
            {
                client.Dispose();
            }
            
            return base.OnStopModuleAsync(services);
        }

        private async Task CreateExchanges(IEnumerable<ExchangeMeta> definitions)
        {
            foreach (ExchangeMeta exchangeMeta in definitions)
            {
                IBus bus = BusModule.GetBus(exchangeMeta.BusName);
                IExchange exchange = await bus.Advanced.ExchangeDeclareAsync(exchangeMeta);
                
                var createdExchange = new CreatedExchange(bus, exchange, exchangeMeta);
                _createdExchanges[exchangeMeta.MessageType] = createdExchange;
            }
        }

        // Creates a RPC client for each RPC exchange.  RPC style commands are passed through
        // this client when sent.  It also monitors the reply queue for commands responses 
        // that are correlated back to the original sent command.
        private async Task CreateRpcExchangeClients(IEnumerable<ExchangeMeta> definitions)
        {
            var rpcExchanges = definitions.Where(d => d.IsRpcExchange);
            foreach (ExchangeMeta rpcExchange in rpcExchanges)
            {
                string rpcClientKey = $"{rpcExchange.BusName}|{rpcExchange.QueueMeta.QueueName}";
                if (! _exchangeRpcClients.ContainsKey(rpcClientKey))
                {
                    var rpcClient = CreateRpcClient(rpcExchange);
                    _exchangeRpcClients[rpcClientKey] = rpcClient;
                    await rpcClient.CreateAndSubscribeToReplyQueueAsync();
                }
            }
        }
        
        protected virtual IRpcClient CreateRpcClient(ExchangeMeta definition)
        {
            IBus bus = BusModule.GetBus(definition.BusName);
            var rpcClient = new RpcClient(definition.BusName, definition.QueueMeta.QueueName, bus);
            var logger = Context.LoggerFactory.CreateLogger<RpcClient>();

            rpcClient.SetLogger(logger);
            return rpcClient;
        }

        // Called when a reconnection to the the broker is detected.  The IBus will reestablish
        // the connection when available.  However, the reconnection could mean that the broker
        // was restarted.  In this case, any auto-delete queues will need to be recreated.
        private async Task HandleReconnection(ReconnectionEventArgs eventArgs)
        {
            // Recreate all exchanges/queues on the bus for which the connection was restored.
            // RabbitMQ will only create these entities if they are not already present.
            var busExchanges = _messageExchanges.Values
                .Where(e => e.BusName == eventArgs.Connection.BusName)
                .ToArray();
                
            await CreateExchanges(busExchanges);

            // Next, recreate all IRpcClient reply-queues that belong to bus 
            // that was reconnected.
            var busRpcClients = _exchangeRpcClients.Values
                .Where(c => c.BusName == eventArgs.Connection.BusName)
                .ToArray();

            foreach (IRpcClient client in busRpcClients)
            {
                await client.CreateAndSubscribeToReplyQueueAsync();
            }
        }
        
        
        //----------- [Plugin Services] ---------------
        
        // Determines if the specified message should be delivered to an exchange.
        public bool IsExchangeMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            return _messageExchanges.ContainsKey(messageType);
        }

        // Returns information about the exchange to which a given type of message should be delivered.
        public ExchangeMeta GetExchangeMeta(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            if (! IsExchangeMessage(messageType))
            {
                throw new InvalidOperationException(
                    $"No exchange definition registered for message type: {messageType}");
            }
            return _messageExchanges[messageType];
        }

        // Returns exchange to which a given type of message should be delivered.
        public CreatedExchange GetExchange(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            if (! IsExchangeMessage(messageType))
            {
                throw new InvalidOperationException(
                    $"No exchange definition registered for message type: {messageType}");
            }
            return _createdExchanges[messageType];
        }
        
        // Returns the RPC Client used to deliver a message to a consumer for which a response
        // will be received on a corresponding queue and returned to the sender of the command.
        public IRpcClient GetRpcClient(string busName, string queueName)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus not specified.", nameof(busName));
            
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue not not specified.", nameof(queueName));

            string rpcClientKey = $"{busName}|{queueName}";
            if (! _exchangeRpcClients.TryGetValue(rpcClientKey, out IRpcClient client))
            {
                throw new InvalidOperationException(
                    $"RPC client for the queue named: {queueName} on bus: {busName} is not registered.");
            }

            return client;
        }
        
        
        //----------- [Plugin Logging] ---------------

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["PublisherExchanges"] = _messageExchanges.Values.Select(e =>
            {
                var exchangeLog = new Dictionary<string, object>();
                e.LogProperties(exchangeLog);
                return exchangeLog;
            }).ToArray();
        }
    }
}