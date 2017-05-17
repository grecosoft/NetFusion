using FluentAssertions;
using NetFusion.Base;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Linq;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Exchanges
{
    /// <summary>
    /// Publisher applications define exchanges by deriving an application specific
    /// exchange from one the following predefined base exchanges:  TopicExchange,
    /// DirectExchange, WorkQueueExchange, RpcExchange, and FanoutExchange.  the
    /// derived application specific exchange class definitions can override any
    /// default settings specified by the base exchange type.  Each base exchange
    /// type is preconfigured with the most common configuration settings.
    /// </summary>
    public class CommonExchangeTests
    {
        /// <summary>
        /// Json serialization is used by default.  This default setting
        /// can be overridden by a specific exchange definition.
        /// </summary>
        [Fact (DisplayName = nameof(JsonSerialization_UsedByDefault))]
        public void JsonSerialization_UsedByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.ContentType.Should().Be(SerializerTypes.Json);
        }

        /// <summary>
        /// The broker name specified on the application specific exchange 
        /// definition corresponds to the broker settings stored in the
        /// application configuration file.
        /// </summary>
        [Fact (DisplayName = nameof(BrokerName_IsSet))]
        public void BrokerName_IsSet()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.BrokerName.Should().Be("MockTestBrokerName");
        }

        /// <summary>
        /// For critical application queues, the publisher should define the 
        /// needed queues when as part of the exchange definition.  
        /// </summary>
        [Fact (DisplayName = nameof(QueuesCanBeCreated_WhenExchangeCreated))]
        public void QueuesCanBeCreated_WhenExchangeCreated()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();
            exchange.Queues.Should().HaveCount(1);
            exchange.Queues.First().QueueName.Should().Be("FirstMockTestQueue");
        }

        /// <summary>
        /// An exchange can define multiple queues based on the type of exchange.
        /// </summary>
        [Fact (DisplayName = nameof(MultipleQueues_CanBeCreated))]
        public void MultipleQueues_CanBeCreated()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue", SecondQueueName = "SecondMockTestQueue" };
            exchange.Setup();
            exchange.Queues.Should().HaveCount(2);
        }
    }
}
