using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
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
    /// Plugin module responsible for finding and caching the registered exchange definitions 
    /// that should be created when the associated message type is published.  This plugin
    /// module encapsulates the concerns of the publisher.  It finds all exchange definitions
    /// describing the exchanges/queues to which messages can be sent.  This discovered metadata
    /// is then used to create the needed exchanges and queues by delegating to the EasyNetQ
    /// advanced API.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        // Dependent Modules:
        private IBusModule _busModule;

        // Other  plugins (normally application plugins) specify the exchanges 
        // to be created by defining one or more IExchangeRegister derived types.
        // NOTE:  NetFusion finds and instantiates all IExchangeRegistry types.
        public IEnumerable<IExchangeRegistry> Registries { get;  protected set; }  
        
        // Records for a given message type the exchange to which the message should be published.
        private IDictionary<Type, ExchangeMeta> _messageExchanges;

        // Stores for the unique set of named RPC exchanges the associated RPC client that will create a queue,
        // on the default exchange, to process replies from the consumer containing the response to the original
        // sent command.  A client and replay queue is created for each unique RPC exchange name allowing a single
        // queue to process replies from various RPC commands.  It also allows the application to group common
        // commands based on concern or processing distribution needs.
        private readonly IDictionary<string, IRpcClient> _exchangeRpcClients;

        public PublisherModule()
        {
            _exchangeRpcClients = new Dictionary<string, IRpcClient>();
        }

        public override void Configure()
        {
            _busModule = Context.GetPluginModule<IBusModule>();

            var definitions = Registries.SelectMany(r => r.GetDefinitions()).ToArray();

            ApplyConfiguredOverrides(definitions);
            AssertExchangeDefinitions(definitions);

            _messageExchanges = definitions.ToDictionary(m => m.MessageType);
        } 

        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {
            CreateExchangeRpcClients(_messageExchanges.Values);
            
            foreach (IRpcClient client in _exchangeRpcClients.Values)
            {
                await client.CreateAndSubscribeToReplyQueueAsync();
            }
        }

        protected override Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach(IRpcClient client in _exchangeRpcClients.Values)
            {
                client.Dispose();
            }
            
            return base.OnStopModuleAsync(services);
        }

        public bool IsExchangeMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            return _messageExchanges.ContainsKey(messageType);
        }

        public ExchangeMeta GetDefinition(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            if (! IsExchangeMessage(messageType))
            {
                throw new InvalidOperationException(
                    $"No exchange definition registered for message type: {messageType}");
            }
            return _messageExchanges[messageType];
        }

        // All exchange types:  Direct, Topic, and Fan-out are defined within code with the most common default
        // values.  These default values can be overridden by specifying them within the application's configuration.
        private void ApplyConfiguredOverrides(IEnumerable<ExchangeMeta> definitions)
        {
            foreach (var definition in definitions)
            {
                _busModule.ApplyExchangeSettings(definition);
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
            // The same exchange name can be used as long as associated with a different named buses.
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
                    "Queue names must be unique for a given named bus.  The following have been configured more than once.", 
                    "duplicate-queues", duplicateQueueNames);
            } 
        }

        private static void AssertNoDuplicateMessageTypes(IEnumerable<ExchangeMeta> definitions)
        {
            var duplicateMessages = definitions.GroupBy(d => d.MessageType)
                .Where(g => g.Count() > 1)
                .Select(g => new {
                    MessageType = g.Key,
                    Definition = g.Select(d => $"Exchange: {d.ExchangeName}, Queue: {d.QueueMeta?.QueueName}")
                }).ToArray();

            if (duplicateMessages.Any())
            {
                throw new ContainerException(
                    "Message type can only be associated with one exchange or queue.", 
                    "duplicate-message-types", duplicateMessages);
            } 
        }

        // Creates a RPC client for each RPC exchange.  RPC style commands are passed through
        // this client when sent.  It also monitors the reply queue for commands responses 
        // that are correlated back to the original sent command.
        private void CreateExchangeRpcClients(IEnumerable<ExchangeMeta> definitions)
        {
            var rpcExchanges = definitions.Where(d => d.IsRpcExchange);
            foreach (ExchangeMeta rpcExchange in rpcExchanges)
            {
                string rpcClientKey = $"{rpcExchange.BusName}|{rpcExchange.QueueMeta.QueueName}";
                if (! _exchangeRpcClients.ContainsKey(rpcClientKey))
                {
                    _exchangeRpcClients[rpcClientKey] = CreateRpcClient(rpcExchange);
                }
            }
        }

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

        protected virtual IRpcClient CreateRpcClient(ExchangeMeta definition)
        {
            IBus bus = _busModule.GetBus(definition.BusName);
            var rpcClient = new RpcClient(definition.BusName, definition.QueueMeta.QueueName, bus);
            var logger = Context.LoggerFactory.CreateLogger<RpcClient>();

            rpcClient.SetLogger(logger);
            return rpcClient;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Publisher:Exchanges"] = _messageExchanges.Values.Select(e =>
            {
                var exchangeLog = new Dictionary<string, object>();
                e.LogProperties(exchangeLog);
                return exchangeLog;
            }).ToArray();
        }
    }
}