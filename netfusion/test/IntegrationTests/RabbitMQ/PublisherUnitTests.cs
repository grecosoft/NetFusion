using EasyNetQ.Topology;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.Test.Container;
using Xunit;

namespace IntegrationTests.RabbitMQ
{

    /// <summary>
    /// Tests to assert the default configuration for each of the
    /// common exchange syles are correctly initialized.  After the
    /// the default configuration values are specified, any settings
    /// existing within the application's settings are then applied.
    /// </summary>
    public class PublisherUnitTests
    {
        /// <summary>
        /// A direct exchange delivers events based on it specified RouteKey value.
        /// The route key is a string value used to determine the subscribed queues
        /// to which the event should be delivered.  The publisher defines the exchange
        /// and the subscribers define the queues.  The subscriber specifies the
        /// RouteKey values for each queue to which matching events should be delivered.
        /// If multiple subscribers are connected the the same queue, the event will be
        /// delivered round-robin to each connection client.  
        /// </summary>
        [Fact]
        public void DirectExchangeStyle_BaseConfigurations()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(r =>
                    {
                        r.WithRabbitMqHost(typeof(ExchangeRegistryUnderTest));
                    })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(DirectDomainEvent));
                        Assert.NotNull(definition);
                        
                        // Asserts the proper configuration of a Direct exchange.  A subset of
                        // these properties can be overridden from the application configuration.
                      
                        Assert.Equal("TestBus1", definition.BusName);
                        Assert.Equal("DirectExchangeName", definition.ExchangeName);
                        Assert.Null(definition.QueueMeta);
                        Assert.Null(definition.RouteKey);
                        Assert.Equal("application/json", definition.ContentType);
                        Assert.Equal(ExchangeType.Direct, definition.ExchangeType);
                        Assert.Null(definition.AlternateExchangeName);
                        Assert.Equal(typeof(DirectDomainEvent), definition.MessageType);

                        Assert.False(definition.IsDefaultExchange);
                        Assert.False(definition.IsAutoDelete);
                        Assert.True(definition.IsDurable);
                        Assert.False(definition.IsPassive);
                        Assert.True(definition.IsPersistent);
                        Assert.False(definition.IsRpcExchange);
                    });
            });
        }
        
        /// <summary>
        /// A topic exchange is similar to a direct exchange but the RouteKey, assocated with the
        /// event, contains a period delimited value.  When a message is delivered to a topic
        /// exchange, it is delivered to all queues bound with a pattern matching the RouteKey.
        /// Example:  [VW.GOLF.GTI, VW.GOLF.ALLTRACK -> VW.GOLF.*, VW.*.*]
        /// The publisher defines the exchange and the subscribers define the queues.  The subscriber
        /// specifies the Pattern values for each queue to which matching events should be delivered.
        /// If multiple subscribers are connected the the same queue, the event will be delivered
        /// round-robin to each connection client.  
        /// </summary>
        [Fact]
        public void TopicExchangeStyle_BaseConfiguration()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost(typeof(ExchangeRegistryUnderTest));
                    })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(TopicDomainEvent));
                        Assert.NotNull(definition);
                        
                        // Asserts the proper configuration of a Topic exchange.  A subset of
                        // these properties can be overridden from the application configuration.
                      
                        Assert.Equal("TestBus1", definition.BusName);
                        Assert.Equal("TopicExchangeName", definition.ExchangeName);
                        Assert.Null(definition.QueueMeta);
                        Assert.Null(definition.RouteKey);
                        Assert.Equal("application/json", definition.ContentType);
                        Assert.Equal(ExchangeType.Topic, definition.ExchangeType);
                        Assert.Null(definition.AlternateExchangeName);
                        Assert.Equal(typeof(TopicDomainEvent), definition.MessageType);

                        Assert.False(definition.IsDefaultExchange);
                        Assert.False(definition.IsAutoDelete);
                        Assert.True(definition.IsDurable);
                        Assert.False(definition.IsPassive);
                        Assert.True(definition.IsPersistent);
                        Assert.False(definition.IsRpcExchange);
                    });
            });
        }
        
        /// <summary>
        /// Unlike the Direct and Topic exchanges, domain events sent to a Fanout exchange syle
        /// of exchange do not have an assocated route key value.  Also, the message is delivered
        /// to all queues defined on the exchange and not round-robin.  Subscribers uses this type
        /// of exchange to be notified of events happening in the domain.  Unlike the Direct and
        /// Topic exchanges, a Fanout exchange is configured so messages are only delivered to
        /// clients when connected to the queue.  Subscribers of this type of exchange are only
        /// concerned about events happening when they are connected and not any prior missed
        /// events.  Given this, the exchanges and queues are automatically created and deleted
        /// as publishers connect and disconnect. 
        /// </summary>
        [Fact]
        public void FanoutExchangeStyle_BaseConfiguration()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost(typeof(ExchangeRegistryUnderTest));
                    })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(FanoutDomainEvent));
                        Assert.NotNull(definition);
                        
                        // Asserts the proper configuration of a Fanout exchange.  A subset of
                        // these properties can be overridden from the application configuration.
                      
                        Assert.Equal("TestBus1", definition.BusName);
                        Assert.Equal("FanoutExchangeName", definition.ExchangeName);
                        Assert.Null(definition.QueueMeta);
                        Assert.Null(definition.RouteKey);
                        Assert.Equal("application/json", definition.ContentType);
                        Assert.Equal(ExchangeType.Fanout, definition.ExchangeType);
                        Assert.Null(definition.AlternateExchangeName);
                        Assert.Equal(typeof(FanoutDomainEvent), definition.MessageType);

                        Assert.False(definition.IsDefaultExchange);
                        Assert.True(definition.IsAutoDelete);
                        Assert.False(definition.IsDurable);
                        Assert.False(definition.IsPassive);
                        Assert.False(definition.IsPersistent);
                        Assert.False(definition.IsRpcExchange);
                    });
            });
        }

        /// <summary>
        /// Commands are delivered to Work Queues.  A work queue is subscribed by the consumer
        /// that will process the received command.  Technically this message delivery method
        /// does not have an exchange and messages are delivered to queues with a name matching
        /// the value of the command's route key.
        /// </summary>
        [Fact]
        public void WorkQueueStyle_BaseConfiguration()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost(typeof(ExchangeRegistryUnderTest));
                    })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(WorkQueueCommand));
                        Assert.NotNull(definition);
                        
                        // Asserts the proper configuration of a Workqueue.  A subset of
                        // these properties can be overridden from the application configuration.
                      
                        Assert.Equal("TestBus1", definition.BusName);
                        Assert.Equal("WorkQueueName", definition.QueueMeta.QueueName);
                        Assert.Null(definition.ExchangeName);
                        Assert.Equal("WorkQueueName", definition.RouteKey);
                        Assert.Equal("application/json", definition.ContentType);
                        Assert.Null(definition.ExchangeType);
                        Assert.Null(definition.AlternateExchangeName);
                        Assert.Equal(typeof(WorkQueueCommand), definition.MessageType);

                        Assert.True(definition.IsDefaultExchange);
                        
                        // These will be all False since they don't apply to a Workqueue since
                        // it is created on the default exchange.
                        Assert.False(definition.IsAutoDelete);
                        Assert.False(definition.IsDurable);
                        Assert.False(definition.IsPassive);
                        Assert.False(definition.IsPersistent);
                        Assert.False(definition.IsRpcExchange);
                    });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void RpcExchangeStyle_BaseConfiguration()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithRabbitMqHost(typeof(ExchangeRegistryUnderTest));
                    })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(RpcCommand));
                        Assert.NotNull(definition);
                      
                        Assert.Equal("TestBus1", definition.BusName);
                        Assert.Equal("RpcExchangeName", definition.QueueMeta.QueueName);
                        Assert.Null(definition.RouteKey);
                        Assert.Equal("application/json", definition.ContentType);
                        Assert.Null(definition.AlternateExchangeName);
                        Assert.Equal(typeof(RpcCommand), definition.MessageType);

                        // Rpc Commands are sent to a queue defined on the default exchange.
                        // The Route-Key of the message determines the exchange to which it
                        // is published and consumed by subscriber.  The Action-Namesapce is
                        // a string key used to identify the the commands and used by the
                        // subscriber to route the message to the correct in-process handler.
                        // This allows for efficient use of queues since several RPC messages
                        // can be sent on a queue.
                        Assert.True(definition.IsDefaultExchange);
                        Assert.True(definition.IsRpcExchange);
                        Assert.Equal("ActionName", definition.ActionNamespace);
                        Assert.True(definition.QueueMeta.IsAutoDelete);
                        Assert.False(definition.QueueMeta.IsDurable);
                        Assert.False(definition.QueueMeta.IsPassive);
                    });
            });
        }

        public class ExchangeRegistryUnderTest : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineDirectExchange<DirectDomainEvent>("DirectExchangeName", "TestBus1");
                DefineTopicExchange<TopicDomainEvent>("TopicExchangeName", "TestBus1");
                DefineFanoutExchange<FanoutDomainEvent>("FanoutExchangeName", "TestBus1");
                DefineWorkQueue<WorkQueueCommand>("WorkQueueName", "TestBus1");
                DefineRpcQueue<RpcCommand>("RpcExchangeName", "ActionName", "TestBus1");
            }
        }

        private class DirectDomainEvent : DomainEvent
        {
            
        }
        
        private class TopicDomainEvent : DomainEvent
        {
            
        }
        
        private class FanoutDomainEvent : DomainEvent
        {
            
        }
        
        private class WorkQueueCommand : Command
        {
            
        }

        private class RpcCommand : Command<int>
        {
            
        }
    }
}