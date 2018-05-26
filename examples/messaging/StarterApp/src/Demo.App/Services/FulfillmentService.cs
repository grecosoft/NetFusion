using Demo.App.DomainEvents;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging;

namespace Demo.App.Service
{
    public class FulfillmentService 
        : IMessageConsumer
    {
        private readonly ILogger _logger;

        public FulfillmentService(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(
                "Registration Fulfillment");
        }

        [InProcessHandler]
        public void FinishRegistration(
            RegistrationPassedEvent passedEvent)
        {
            _logger.LogDebug("Reference Number: {refNum}",
                 passedEvent.ReferenceNumber);

            // React to domain-event by calling repositories,
            // services and domain-entities.
        }
    }
}