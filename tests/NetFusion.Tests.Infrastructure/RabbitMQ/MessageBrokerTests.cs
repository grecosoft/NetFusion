using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
{
    public class MessageBrokerTests
    {
        [Fact]
        public void ExceptionIfExchangeBrokerNameNoConfigured()
        {

        }

        [Fact]
        public void EachDiscoveredExchangeIsCreatedOnRabbitServer()
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
