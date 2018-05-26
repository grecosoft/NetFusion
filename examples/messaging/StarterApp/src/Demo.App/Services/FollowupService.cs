using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.App.DomainEvents;
using Demo.Infra;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.App.Service
{
    public class FollowupService
        : IMessageConsumer
    {
        private readonly ILogger _logger;
        private readonly IRegistrationDataAdapter _adapter;

        public FollowupService(
            ILoggerFactory loggerFactory,
            IRegistrationDataAdapter adapter)
        {
            _logger = loggerFactory.CreateLogger("Registration Followup");
            _adapter = adapter;
        }

        [InProcessHandler]
        public async Task CheckSimilarModel(
            RegistrationFailedEvent failedEvent)
        {
            if (failedEvent.Make == "Yugo")
            {
                // Exception used to show resulting log:
                throw new InvalidOperationException(
                    "Registration Failed.");
            }

            var models = await _adapter.GetValidModelsAsync(failedEvent.Year);

            var makeModels = models.Where(m => m.Make == failedEvent.Make)
                .ToArray();

            // React to domain-event by calling repositories,
            // services and domain-entities.
            _logger.LogDebug(makeModels.ToIndentedJson());
        }
    }
}