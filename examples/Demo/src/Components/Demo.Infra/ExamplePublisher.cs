using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types;

namespace Demo.Infra
{
    public class ExamplePublisher : MessagePublisher
    {
        private readonly ILogger<ExamplePublisher> _logger;
        public override IntegrationTypes IntegrationType => IntegrationTypes.Internal;

        public ExamplePublisher(ILogger<ExamplePublisher> logger)
        {
            _logger = logger;
        }

        public override Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Message Correlation ID ==> {message.GetCorrelationId()}");
            _logger.LogDebug(message.ToIndentedJson());

            return Task.CompletedTask;
        }
    }
}

