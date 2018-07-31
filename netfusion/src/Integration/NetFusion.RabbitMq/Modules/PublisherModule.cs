using System;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plugin module responsible for finding and caching the registered exchange definitions 
    /// that should be created when the associated message type is published.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        // Dependent Services:
        private IBusModule _busModule;
        
        // Records for a given message type the exchange to which the message
        // should be published.
        private IDictionary<Type, ExchangeDefinition> _messageExchanges;

        // Stores for the unique set of named RPC exchanges the associated RPC client
        // that will create a queue, on the default exchange, to process replies from
        // the consumer containing the response to the original send command. 
        private readonly IDictionary<string, IRpcClient> _exchangeRpcClients;

        // Other  plugins (normally application plugins) specify the exchanges 
        // to be created by defining one or more IExchangeRegister derived types.
        public IEnumerable<IExchangeRegistry> Registries { get;  protected set; }  

        
        public PublisherModule()
        {
            _exchangeRpcClients = new Dictionary<string, IRpcClient>();
        }

        public override void Configure()
        {
            _busModule = Context.GetPluginModule<IBusModule>();

            var definitions = Registries.SelectMany(r => r.GetDefinitions()).ToArray();

            CheckRabbitMqPublisherRegistered(definitions);
            ApplyConfiguredOverrides(definitions);
            AssertExchangeDefinitions(definitions);
            SetExchangeRpcClients(definitions);

            _messageExchanges = definitions.ToDictionary(m => m.MessageType);
        } 

        public override void StartModule(IServiceProvider services)
        {
            foreach (IRpcClient client in _exchangeRpcClients.Values)
            {
                client.CreateAndSubscribeToReplyQueue();
            }
        }

        // Unique value set at development time identifying the host plugin.
        // The value can be used to tag exchanges and queues so the associated
        // host can be identified.  This will also make a given queue name 
        // unique to a given application host.
        public string PublishingHostId => Context.AppHost.Manifest.PluginId;

        public bool IsExchangeMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            return _messageExchanges.ContainsKey(messageType);
        }

        public ExchangeDefinition GetDefinition(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            if (! IsExchangeMessage(messageType))
            {
                throw new InvalidOperationException(
                    $"No exchange definition registered for message type: {messageType}");
            }
            return _messageExchanges[messageType];
        }

        private void CheckRabbitMqPublisherRegistered(ExchangeDefinition[] definitions)
        {
            // If there are no defined exchange definitions, the application does not
            // need to have the RabbitMq publisher registered.
            if (definitions.Length == 0) return;

            var messageModule = Context.GetPluginModule<IMessageDispatchModule>();

            Type publisher = messageModule.DispatchConfig.PublisherTypes
                .FirstOrDefault(pt => pt == typeof(RabbitMqPublisher));

            if (publisher == null)
            {
                Context.Logger.LogWarning(RabbitMqLogEvents.PublisherEvent,
                    $"Exchanges have been declared but the publisher of type: {typeof(RabbitMqPublisher)} " + 
                     "has not been registered.  For messages to be published, this class must be registered." );
            }
        }

        // All exchange types:  Direct, Topic, and Fanout are defined within code with the most common default
        // values.  These default values can be overridden by specifying them within the application's configuration.
        private void ApplyConfiguredOverrides(ExchangeDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                if (definition.ExchangeName != null)
                {
                    var exchangeSettings = _busModule.GetExchangeSettings(definition.BusName, definition.ExchangeName);
                    if (exchangeSettings != null)
                    {
                        definition.ApplyOverrides(exchangeSettings);
                    }
                }
            }
        }

        private static void AssertExchangeDefinitions(ExchangeDefinition[] definitions)
        {
            AssertNoDuplicateExchanges(definitions);
            AssertNoDuplicateQueues(definitions);
            AssertNoDuplicateMessageTypes(definitions);
        }

        private static void AssertNoDuplicateExchanges(ExchangeDefinition[] definitions)
        {
            // All exchange names that are not RPC exchanges must be unique for given named bus.
            // The same exchange name can be used as long as associated with different named buses.
            var duplidateExchangeNames = definitions.Where(
                    d => d.ExchangeName != null && !d.IsRpcExchange)
                .WhereDuplicated(d => new { d.BusName,  d.ExchangeName});
            
            if (duplidateExchangeNames.Any())
            {
                throw new ContainerException(
                    "Exchange names must be unique for a given named bus.  The following have been configured " +
                    "more than once.", "duplicate-exchanges", duplidateExchangeNames);
            }
        }
        
        private static void AssertNoDuplicateQueues(ExchangeDefinition[] definitions)
        {
            var duplicateQueueNames = definitions.Where(d => d.QueueName != null)
                .WhereDuplicated(d => new { d.BusName, d.QueueName });
            
            if (duplicateQueueNames.Any())
            {
                throw new ContainerException(
                    "Queue names must be unique.  The following have been configured more than once.", 
                    "duplicate-queues", duplicateQueueNames);
            } 
        }

        private static void AssertNoDuplicateMessageTypes(IEnumerable<ExchangeDefinition> definitions)
        {
            var duplicateMessages = definitions.GroupBy(d => d.MessageType)
                .Where(g => g.Count() > 1)
                .Select(g => new {
                    MessageType = g.Key,
                    Definition = g.Select(d => $"Exchange: {d.ExchangeName}, Queue: {d.QueueName}")
                }).ToArray();

            if (duplicateMessages.Any())
            {
                throw new ContainerException(
                    "Message type can only be associated with one exchange or queue.", 
                    "duplicate-message-types", duplicateMessages);
            } 
        }

        // Creates a RPC client for each RPC exchange.  RPC style commands are passed through
        // this client when publishing.  It also minitors the reply queue for commands responses 
        // that are correlated back to the original sent command.
        private void SetExchangeRpcClients(ExchangeDefinition[] definitions)
        {
            var rpcExchanges = definitions.Where(d => d.IsRpcExchange);
            foreach (ExchangeDefinition rpcExchange in rpcExchanges)
            {
                string rpcClientkey = $"{rpcExchange.BusName}|{rpcExchange.ExchangeName}";
                if (! _exchangeRpcClients.ContainsKey(rpcClientkey))
                {
                    _exchangeRpcClients[rpcClientkey] = CreateRpcClient(rpcExchange);
                }
            }
        }

        public IRpcClient GetRpcClient(string busName, string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus not specified.", nameof(busName));
            
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange not not specified.", nameof(exchangeName));

            string rpcClientkey = $"{busName}|{exchangeName}";
            if (! _exchangeRpcClients.TryGetValue(rpcClientkey, out IRpcClient client))
            {
                throw new InvalidOperationException(
                    $"RPC client for the exchange named: {exchangeName} on bus: {busName} is not registered.");
            }

            return client;
        }

        protected virtual IRpcClient CreateRpcClient(ExchangeDefinition definition)
        {
            IBus bus = _busModule.GetBus(definition.BusName);
            var rpcClient = new RpcClient(definition.BusName, definition.ExchangeName, bus);
            var logger = Context.LoggerFactory.CreateLogger<RpcClient>();

            rpcClient.SetLogger(logger);
            return rpcClient;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Exchanges-Queues"] = _messageExchanges.Values.Select( e => new {
                MessageType = e.MessageType.ToString(),
                e.BusName,
                ExchangeName = e.ExchangeName ?? "(Default-Exchange)",
                e.QueueName,
                ScriptPredicate = new {
                    e.ScriptPredicate?.ScriptName,
                    e.ScriptPredicate?.AttributeName
                },
                ExchangeConfig = new {
                    e.ExchangeType,
                    e.AlternateExchangeName,
                    e.IsDefaultExchangeQueue,
                    e.IsAutoDelete,
                    e.IsDurable,
                    e.IsPersistent,
                    e.ContentType
                }
            }).ToArray();
        }
    }
}