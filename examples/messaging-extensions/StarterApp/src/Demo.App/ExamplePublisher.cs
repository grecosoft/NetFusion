using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;

namespace Demo.App
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
            _logger.LogWarning($"Message Correlation ID ==> {message.GetCorrelationId()}");
            _logger.LogWarning(message.ToIndentedJson());

            return Task.CompletedTask;
        }
    }
}
