using System;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.Test.Container;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    /// <summary>
    /// The publisher module contains the logic specific to creating exchanges/queues
    /// defined by the publishing host.
    /// </summary>
    public class PublisherModuleTests
    {
        /// <summary>
        /// The publishing application can determine if a given message has an assocated
        /// exchange.  The publishing application defines exchanges by declaring one or
        /// more IExchangeRegistry or ExchangeRegistryBase classes.  This information is
        /// queried and cached when the NetFusion.RabbitMQ plugin is bootstrapped.
        /// </summary>
        [Fact]
        public void CanDetermineIfMessage_HasAnAssociatedExchange()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(ValidExchangeRegistry)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var isExchangeMsg = m.IsExchangeMessage(typeof(AutoSalesEvent));
                        Assert.True(isExchangeMsg);
                    });
            });
        }

        /// <summary>
        /// When publishing a message with an assocated exchange, the exchange definition
        /// is retreived and used to declare the exchange on the RabbitMQ message bus.
        /// </summary>
        [Fact]
        public void CanLookupExchangeDefinition_ForMessageType()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(ValidExchangeRegistry)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var definition = m.GetDefinition(typeof(AutoSalesEvent));
                        Assert.NotNull(definition);
                    });
            });
        }

        [Fact]
        public void RequestingExchangeDefinition_ForNotExchangeMessageType_RaisesException()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Resolver(r => { r.WithRabbitMqHost(typeof(ValidExchangeRegistry)); })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c =>
                    {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) =>
                        {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        Assert.Throws<InvalidOperationException>(() => m.GetDefinition(typeof(TestCommand1)));                       
                    });
            });
        }
        
        /// <summary>
        /// All exchange names defined a specific message bus must be unique for a given configured host. 
        /// </summary>
        [Fact]
        public void ExchangeName_MustBe_Unique_OnSameBus()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Resolver(r =>
                    {
                        r.WithRabbitMqHost(typeof(DuplicateExchangeRegistry));
                    })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c => {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) => {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        
                    });
            });
        }
        
        /// <summary>
        /// The same exchange names can be used across different configured hosts.
        /// </summary>
        [Fact]
        public void ExchangeName_CanBeTheSame_OnDifferentBusses()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Resolver(r =>
                    {
                        r.WithRabbitMqHost(typeof(ValidDuplicateExchangeRegistry));
                    })
                    .Configuration(TestSetup.AddValidMultipleBusConfig)
                    .Container(c => {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) => {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        Assert.NotNull(m.GetDefinition(typeof(TestDomainEvent)));
                        Assert.NotNull(m.GetDefinition(typeof(TestDomainEvent2)));
                    });
            });
        }

        /// <summary>
        /// All queue names defined a specific message bus must be unique. 
        /// </summary>
        [Fact]
        public void QueueName_MustBe_Unique_OnSameBus()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Resolver(r =>
                    {
                        r.WithRabbitMqHost(typeof(DuplicateQueueRegistry));
                    })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c => {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) => {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        
                    });
            });
        }
        
        [Fact]
        public void QueueName_CanBeTheSame_OnDifferentBusiness()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Resolver(r =>
                    {
                        r.WithRabbitMqHost(typeof(ValidDuplicateQueueRegistry));
                    })
                    .Configuration(TestSetup.AddValidMultipleBusConfig)
                    .Container(c => {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) => {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        Assert.NotNull(m.GetDefinition(typeof(TestCommand1)));
                        Assert.NotNull(m.GetDefinition(typeof(TestCommand2)));
                    });
            });
        }

        /// <summary>
        /// For each specified RPC exchange, a corresponding IRpcClient instance is created for
        /// sending commands to the subscriber.  Multiple commands can be sent on the same RPC
        /// eachange where the RouteKey identifies the specific command.  For example, there can
        /// be a "Calculations" exchange on which commands with route keys "CalculatePropTax" and
        /// "CalculateAutoTax" can be published.  Note:  A Direct RabbitMQ exchange is used for
        /// sending RPC style messages.  In this example, a single IRpcClient is created for the
        /// "Calculations" exchange over which the "CalculatePropTax" and "CalculateAutoTax"
        /// commands are sent.  Each IRpcClient instance creates a corresponding queue on which
        /// it will subscribe for responses to the sent commands.  This design allows multiple
        /// IRpcClient instances to be created to allow dispursing commands accross eachanges.
        /// </summary>
        [Fact]
        public void RpcClientCreated_ForEachRpcExchange()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange
                    .Resolver(r =>
                    {
                        r.WithRabbitMqHost(typeof(RpcExchangeRegistry));
                    })
                    .Configuration(TestSetup.AddValidBusConfig)
                    .Container(c => {
                        c.WithConfig((MessageDispatchConfig dispatchConfig) => {
                            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
                        });
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.PluginModule<MockPublisherModule>(m =>
                    {
                        var calExchangeClient = m.GetRpcClient("TestBus1", "Calculations");
                        var refExchangeClient = m.GetRpcClient("TestBus1", "LookupReferenceData");
                        
                        Assert.NotNull(calExchangeClient);
                        Assert.NotNull(refExchangeClient);
                        Assert.NotSame(calExchangeClient, refExchangeClient);
                    });
            });
        }


        public class ValidExchangeRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineTopicExchange<AutoSalesEvent>("AutoSales", "TestBus1");
            }
        }

        public class DuplicateExchangeRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineTopicExchange<TestDomainEvent>("AutoSales", "TestBus1");
                DefineTopicExchange<TestDomainEvent2>("AutoSales", "TestBus1");
            }
        }
        
        public class ValidDuplicateExchangeRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineTopicExchange<TestDomainEvent>("AutoSales", "TestBus1");
                DefineTopicExchange<TestDomainEvent2>("AutoSales", "TestBus2");
            }
        }

        public class DuplicateQueueRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineWorkQueue<TestCommand1>("GenerateInvoice", "TestBus1");
                DefineWorkQueue<TestCommand2>("GenerateInvoice", "TestBus1");
            }
        }
        
        public class ValidDuplicateQueueRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineWorkQueue<TestCommand1>("GenerateInvoice", "TestBus1");
                DefineWorkQueue<TestCommand2>("GenerateInvoice", "TestBus2");
            }
        }

        public class RpcExchangeRegistry : ExchangeRegistryBase
        {
            protected override void OnRegister()
            {
                DefineRpcExchange<CalculatePropTax>("Calculations", "CalculatePropTax", "TestBus1");
                DefineRpcExchange<CalculateAutoTax>("Calculations", "CalculateAutoTax", "TestBus1");
                DefineRpcExchange<GetTaxRates>("LookupReferenceData", "GetTaxRates", "TestBus1");
            }
        }

        public class AutoSalesEvent : DomainEvent
        {
        
        }
    
        public class TestCommand1 : Command
        {

        }
    
        public class TestCommand2 : Command
        {

        }
    
        public class TestDomainEvent : DomainEvent
        {

        }

        public class TestDomainEvent2 : DomainEvent
        {
            
        }

        public class CalculatePropTax : Command
        {

        }
    
        public class CalculateAutoTax : Command
        {

        }
    
        public class GetTaxRates : Command
        {

        }
    }
}