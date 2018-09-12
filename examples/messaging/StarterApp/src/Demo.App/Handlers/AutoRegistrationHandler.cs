using System;
using System.Threading.Tasks;
using Demo.Domain.Commands;
using Demo.Domain.Events;
using Demo.Domain.Entities;
using Demo.Domain.Adapters;
using System.Linq;
using NetFusion.Messaging;

namespace Demo.App.Handlers
{
    public class AutoRegistrationHandler : IMessageConsumer
    {
        private readonly IMessagingService _messaging;
        private readonly IRegistrationDataAdapter _adapter;

        public AutoRegistrationHandler(
            IMessagingService messaging,
            IRegistrationDataAdapter adapter)
        {
            _messaging = messaging;
             _adapter = adapter;
        }

        [InProcessHandler]
        public async Task<RegistrationStatus> RegisterAuto(RegisterAutoCommand command)
        {
            if (command.Year < 2000)
            {
                throw new InvalidOperationException(
                    "Cars must be newer than the 2000 model year.");
            }

            AutoInfo[] validModels = await _adapter.GetValidModelsAsync(command.Year);

            var status = new RegistrationStatus {
                IsSuccess = IsValidMakeAndModel(command, validModels),
                ReferenceNumber = Guid.NewGuid().ToString(),
                DateAccountActive = DateTime.UtcNow
            };

            await NotifyPassedRegistration(status, command);
            await NotifyFailedRegistration(status, command);

            return status;
        }

        private static bool IsValidMakeAndModel(RegisterAutoCommand command,
            AutoInfo[] validModels)
        {
            return validModels.Any(
                m => m.Make == command.Make &&
                m.Model == command.Model);
        }

        private Task NotifyPassedRegistration(
            RegistrationStatus status, 
            RegisterAutoCommand command)
        {
            if (status.IsSuccess)
            {
                var passedEvent = new RegistrationPassedEvent(
                    status.ReferenceNumber, command.State);

                return _messaging.PublishAsync(passedEvent);
            }
            return Task.CompletedTask;
        }

        private Task NotifyFailedRegistration(
            RegistrationStatus status, 
            RegisterAutoCommand command)
        {
            if (!status.IsSuccess)
            {
                var failedEvent = new RegistrationFailedEvent(
                    status.ReferenceNumber, 
                    command.Make, command.Model,
                    command.Year);

                return _messaging.PublishAsync(failedEvent);
            }
            return Task.CompletedTask;
        }
    }
}
