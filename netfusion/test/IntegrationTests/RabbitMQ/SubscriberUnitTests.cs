using System.Linq;
using EasyNetQ.Topology;
using NetFusion.Messaging;
using NetFusion.Messaging.Plugin.Configs;
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
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        Assert.True(m.CreatedQueues.Count == 5);
                        Assert.True(m.Subscribers.Count == 5);
                    });
            }, TestSetup.AddValidBusConfig);
        }

        [Fact]
        public void DirectQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.QueueName == "TestDirectQueue");
                        Assert.NotNull(queueDef);
                        
                        var exchangeDef = queueDef.Exchange;
                        Assert.NotNull(exchangeDef);

                        // Assert the exchange settings:
                        Assert.Null(exchangeDef.AlternateExchangeName);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestDirectExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Direct, exchangeDef.ExchangeType);
                        Assert.False(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.True(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        Assert.True(queueDef.RouteKeys.Length == 1);
                        Assert.True(queueDef.RouteKeys.FirstOrDefault() == "CustomerRegistrationCompleted");
                        Assert.True(queueDef.AppendHostId);
                        Assert.False(queueDef.AppendUniqueId);
                        Assert.False(queueDef.IsAutoDelete);
                        Assert.True(queueDef.IsDurable);
                        Assert.False(queueDef.IsExclusive);
                        Assert.False(queueDef.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("DirectQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            }, TestSetup.AddValidBusConfig);
        }
        
        [Fact]
        public void TopicQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.QueueName == "TestTopicQueue");
                        Assert.NotNull(queueDef);
                        
                        var exchangeDef = queueDef.Exchange;
                        Assert.NotNull(exchangeDef);
                        
                        // Assert the exchange settings:
                        Assert.Null(exchangeDef.AlternateExchangeName);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestTopicExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Topic, exchangeDef.ExchangeType);
                        Assert.False(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.True(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        Assert.True(queueDef.RouteKeys.Length == 1);
                        Assert.True(queueDef.RouteKeys.FirstOrDefault() == "VW.*.White");
                        Assert.True(queueDef.AppendHostId);
                        Assert.False(queueDef.AppendUniqueId);
                        Assert.False(queueDef.IsAutoDelete);
                        Assert.True(queueDef.IsDurable);
                        Assert.False(queueDef.IsExclusive);
                        Assert.False(queueDef.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("TopicQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            }, TestSetup.AddValidBusConfig);
        }
        
        [Fact]
        public void FanOutQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {

                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.Exchange.ExchangeName == "TestFanoutExchange");
                        Assert.NotNull(queueDef);

                        var exchangeDef = queueDef.Exchange;
                        Assert.NotNull(exchangeDef);
                        
                        // Assert the exchange settings:
                        Assert.Null(exchangeDef.AlternateExchangeName);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.Equal("TestFanoutExchange", exchangeDef.ExchangeName);
                        Assert.Equal(ExchangeType.Fanout, exchangeDef.ExchangeType);
                        Assert.True(exchangeDef.IsAutoDelete);
                        Assert.False(exchangeDef.IsDefaultExchange);
                        Assert.False(exchangeDef.IsDurable);
                        Assert.False(exchangeDef.IsPassive);
                        
                        // Assert the queue settings:
                        Assert.Null(queueDef.RouteKeys);
                        Assert.False(queueDef.AppendHostId);
                        Assert.True(queueDef.AppendUniqueId);
                        Assert.True(queueDef.IsAutoDelete);
                        Assert.False(queueDef.IsDurable);
                        Assert.True(queueDef.IsExclusive);
                        Assert.False(queueDef.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("FanoutQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            }, TestSetup.AddValidBusConfig);
        }
        
        [Fact]
        public void WorkQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.QueueName == "TestWorkQueue");
                        Assert.NotNull(queueDef);
                        
                        Assert.Null(queueDef.RouteKeys);
                        Assert.False(queueDef.AppendHostId);
                        Assert.False(queueDef.AppendUniqueId);
                        Assert.False(queueDef.IsAutoDelete);
                        Assert.True(queueDef.IsDurable);
                        Assert.False(queueDef.IsExclusive);
                        Assert.False(queueDef.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("WorkQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            }, TestSetup.AddValidBusConfig);
        }
        
        [Fact]
        public void RpcQueueCreatedForHandler_WithCorrectConventions()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(c => { c.WithRabbitMqHost(typeof(MockTestBusConsumer)); })
                    .PluginConfig((MessageDispatchConfig c) =>
                    {
                        c.AddPublisher<RabbitMqPublisher>();
                    })
                    .Assert.PluginModule<MockSubscriberModule>(m =>
                    {
                        var queueDef = m.CreatedQueues.FirstOrDefault(q => q.QueueName == "TestRpcQueue");
                        Assert.NotNull(queueDef);

                        var exchangeDef = queueDef.Exchange;
                        Assert.NotNull(exchangeDef);
                                           
                        // Assert the exchange settings:
                        Assert.Null(exchangeDef.AlternateExchangeName);
                        Assert.Equal("TestBus1", exchangeDef.BusName);
                        Assert.True(exchangeDef.IsDefaultExchange);
                        
                        // Assert the queue settings:          
                        Assert.Equal("ActionName", queueDef.Exchange.ActionNamespace);
                        Assert.False(queueDef.AppendHostId);
                        Assert.False(queueDef.AppendUniqueId);
                        Assert.True(queueDef.IsAutoDelete);
                        Assert.False(queueDef.IsDurable);
                        Assert.False(queueDef.IsExclusive);
                        Assert.False(queueDef.IsPassive);
                            
                        // Assert consumer method subscription:
                        var handlerMethod = typeof(MockTestBusConsumer).GetMethod("RpcQueueMessageHandler");
                        var subscriber = m.Subscribers.FirstOrDefault(s => 
                            s.DispatchInfo.MessageHandlerMethod == handlerMethod);
                        
                        Assert.NotNull(subscriber);
                    });
            }, TestSetup.AddValidBusConfig);
        }

        public class MockTestBusConsumer : IMessageConsumer
        {
            [DirectQueue("TestBus1", "TestDirectQueue", "TestDirectExchange", 
                "CustomerRegistrationCompleted")]
            public void DirectQueueMessageHandler(TestDirectDomainEvent evt)
            {
            
            }
            
            [TopicQueue("TestBus1", "TestTopicQueue", "TestTopicExchange", 
                "VW.*.White")]
            public void TopicQueueMessageHandler(TestTopicDomainEvent evt)
            {
            
            }
            
            [FanoutQueue("TestBus1", "TestFanoutExchange")]
            public void FanoutQueueMessageHandler(TestFanoutDomainEvent evt)
            {
            
            }
            
            [WorkQueue("TestBus1", "TestWorkQueue")]
            public void WorkQueueMessageHandler(TestWorkQueueCommand evt)
            {
            
            }
            
            [RpcQueue("TestBus1", "TestRpcQueue", "ActionName")]
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