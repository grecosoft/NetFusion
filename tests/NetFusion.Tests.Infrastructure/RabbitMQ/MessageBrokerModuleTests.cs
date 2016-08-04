using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
{
    public class MessageBrokerModuleTests
    {
        [Fact]
        public void IfExchangesDecaredRabbitMqPublisherMustBeRegistered()
        {

        }

        [Fact]
        public void ModuleResolvesBrokerSettings()
        {

        }

        // A broker name identifies a RabbitMQ instance.  The broker name specified on an
        // exchange declaration determines the RabbitMQ server on which the exchange and
        // is associated queues are created.
        [Fact]
        public void BrokerNameInSettingMustBeUnique()
        {

        }

        [Fact]
        public void ExceptionIfExchangeBrokerNameNoConfigured()
        {

        }

        [Fact]
        public void AllExchangeDefinitionsAreDiscovered()
        {

        }

        [Fact]
        public void EachDiscoveredExchangeIsCreatedOnRabbitServer()
        {

        }

        // Messages consumers are just classes implementing the IMessageConsumer interface that have
        // message handlers marked with the derived QueueConsumerAttribute attributes.  The class
        // implementing the IMessageConsumer interface must also be marked with the BrokerAttribute.
        // The broker attribute specifies the RabbitMQ server on which the consumer's event handlers
        // are bound.   
        [Fact]
        public void AllMessageConsumersDetermined()
        {

        }

        [Fact]
        public void IfConsumerJoinsNonFanoutQueueNewBindingCreatedForConsumer()
        {

        }

        [Fact]
        public void IfConsumerAddsQueueNewQueueIsCreatedExclusivelyForConsumer()
        {

        }

        [Fact]
        public void WhenMessageArrivesDispatchedToCorrespondingConsumerHandler()
        {

        }

        [Fact]
        public void ThereCanBeOneOneSerializerRegisteryRegisteredByHost()
        {

        }

        [Fact]
        public void IfExhangeSpecifiedSerializerNotFoundThenException()
        {

        }

        [Fact]
        public void IfMessageAssociatedWithExchange_PublishedToRabbitMq()
        {

        }

        [Fact]
        public void DefaultJsonAndBinarySerializersAddedIfNotSpecifiedByHost()
        {

        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is acknowledged by 
        // calling a method that stores an indicator in the header of the message.  If message
        // is acknowledged, RabbitMq notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfAck()
        {

        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is rejected by 
        // calling a method that stores an indicator in the header of the message.  Or simply
        // by not acknowledging the message.  If message is not-acknowledged, RabbitMq notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfNotAck()
        {

        }

        [Fact]
        public void MessageIsPublishedToAllAssociatedExchanges()
        {

        }
    }
}
