using System.Linq;
using EasyNetQ.Topology;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.RabbitMQ.Subscriber;
using NetFusion.Test.Container;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class SubscriberUnitTests
    {
        /// <summary>
        /// Consumers define message handlers bound to an exchange by decorating a method with 
        /// a derived SubscriberQueueAttribute.  Each style of queue has a corresponding attribute
        /// accepting parameters specific to the type of queue.  This test validates that a queue
        /// and exchanges are created.
        /// </summary>
        [Fact]
        public void ExchangesAndQueuesCreated_ForEachHandler()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        Assert.True(m.CreatedExchanges.Count == 4);
                        Assert.True(m.CreatedQueues.Count == 5);
                        Assert.True(m.Subscribers.Count == 5);
                    });
            });
        }

        [Fact]
        public void DirectQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        // Assert the exchange settings:
                        var exchangeDef = m.CreatedExchanges.FirstOrDefault(e => e.ExchangeName == "TestDirectExchange");
                        Assert.NotNull(exchangeDef);
                        
                        Assert.Null(exchangeDef.AlternateExchange);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestDirectExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Direct, exchangeDef.ExchangeType);
                        Assert.False(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.True(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Definition.QueueName == "TestDirectQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.True(queueDef.Definition.RouteKeys.Length == 1);
                        Assert.True(queueDef.Definition.RouteKeys.FirstOrDefault() == "CustomerRegistrationCompleted");
                        Assert.True(queueDef.Definition.AppendHostId);
                        Assert.False(queueDef.Definition.AppendUniqueId);
                        Assert.False(queueDef.Definition.IsAutoDelete);
                        Assert.True(queueDef.Definition.IsDurable);
                        Assert.False(queueDef.Definition.IsExclusive);
                        Assert.False(queueDef.Definition.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("DirectQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            });
        }
        
        [Fact]
        public void TopicQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        // Assert the exchange settings:
                        var exchangeDef = m.CreatedExchanges.FirstOrDefault(e => e.ExchangeName == "TestTopicExchange");
                        Assert.NotNull(exchangeDef);
                        
                        Assert.Null(exchangeDef.AlternateExchange);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestTopicExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Topic, exchangeDef.ExchangeType);
                        Assert.False(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.True(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Definition.QueueName == "TestTopicQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.True(queueDef.Definition.RouteKeys.Length == 1);
                        Assert.True(queueDef.Definition.RouteKeys.FirstOrDefault() == "VW.*.White");
                        Assert.True(queueDef.Definition.AppendHostId);
                        Assert.False(queueDef.Definition.AppendUniqueId);
                        Assert.False(queueDef.Definition.IsAutoDelete);
                        Assert.True(queueDef.Definition.IsDurable);
                        Assert.False(queueDef.Definition.IsExclusive);
                        Assert.False(queueDef.Definition.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("TopicQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            });
        }
        
        [Fact]
        public void FanoutQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        // Assert the exchange settings:
                        var exchangeDef = m.CreatedExchanges.FirstOrDefault(e => e.ExchangeName == "TestFanoutExchange");
                        Assert.NotNull(exchangeDef);
                        
                        Assert.Null(exchangeDef.AlternateExchange);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestFanoutExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Fanout, exchangeDef.ExchangeType);
                        Assert.True(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.False(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Definition.QueueName == "TestFanoutQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.Null(queueDef.Definition.RouteKeys);
                        Assert.False(queueDef.Definition.AppendHostId);
                        Assert.True(queueDef.Definition.AppendUniqueId);
                        Assert.True(queueDef.Definition.IsAutoDelete);
                        Assert.False(queueDef.Definition.IsDurable);
                        Assert.True(queueDef.Definition.IsExclusive);
                        Assert.False(queueDef.Definition.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("FanoutQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            });
        }
        
        [Fact]
        public void WorkQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        // Assert the queue settings:
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Definition.QueueName == "TestWorkQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.Null(queueDef.Definition.RouteKeys);
                        Assert.False(queueDef.Definition.AppendHostId);
                        Assert.False(queueDef.Definition.AppendUniqueId);
                        Assert.False(queueDef.Definition.IsAutoDelete);
                        Assert.True(queueDef.Definition.IsDurable);
                        Assert.False(queueDef.Definition.IsExclusive);
                        Assert.False(queueDef.Definition.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("WorkQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            });
        }
        
        [Fact]
        public void RpcQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        // Assert the exchange settings:
                        var exchangeDef = m.CreatedExchanges.FirstOrDefault(e => e.ExchangeName == "TestRpcExchange");
                        Assert.NotNull(exchangeDef);
                        
                        Assert.Null(exchangeDef.AlternateExchange);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestRpcExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Direct, exchangeDef.ExchangeType);
                        Assert.True(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.False(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Definition.QueueName == "TestRpcQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.True(queueDef.Definition.RouteKeys.Length == 1);
                        Assert.True(queueDef.Definition.RouteKeys.FirstOrDefault() == "ActionName");
                        Assert.True(queueDef.Definition.AppendHostId);
                        Assert.False(queueDef.Definition.AppendUniqueId);
                        Assert.True(queueDef.Definition.IsAutoDelete);
                        Assert.False(queueDef.Definition.IsDurable);
                        Assert.False(queueDef.Definition.IsExclusive);
                        Assert.False(queueDef.Definition.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("RpcQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            });
        }

        public class MockTestBusConsumer : IMessageConsumer
        {
            [DirectQueue("TestDirectQueue", "TestDirectExchange", 
                "CustomerRegistrationCompleted", BusName = "TestBus1")]
            public void DirectQueueMessageHandler(TestDirectDomainEvent evt)
            {
            
            }
            
            [TopicQueue("TestTopicQueue", "TestTopicExchange", 
                "VW.*.White", BusName = "TestBus1")]
            public void TopicQueueMessageHandler(TestTopicDomainEvent evt)
            {
            
            }
            
            [FanoutQueue("TestFanoutQueue", "TestFanoutExchange", BusName = "TestBus1")]
            public void FanoutQueueMessageHandler(TestFanoutDomainEvent evt)
            {
            
            }
            
            [WorkQueue("TestWorkQueue", BusName = "TestBus1")]
            public void WorkQueueMessageHandler(TestWorkQueueCommand evt)
            {
            
            }
            
            [RpcQueue("TestRpcQueue", "TestRpcExchange", "ActionName", BusName = "TestBus1")]
            public void RpcQueueMessageHandler(TestRpcCommand evt)
            {
            
            }
        }
        
        public class TestDirectDomainEvent : DomainEvent
        {
            
        }
        
        public class TestTopicDomainEvent : DomainEvent
        {
            
        }

        public class TestFanoutDomainEvent : DomainEvent
        {
            
        }

        public class TestWorkQueueCommand : Command
        {
            
        }

        public class TestRpcCommand : Command
        {
            
        }
    }
}