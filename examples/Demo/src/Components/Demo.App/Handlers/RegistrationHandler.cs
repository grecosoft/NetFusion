using System;
using Demo.Domain.Commands;
using Demo.Domain.Entities;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging;

namespace Demo.App.Handlers
{
    public class RegistrationHandler : IMessageConsumer
    {
        private readonly ILogger _logger;

        public RegistrationHandler(ILogger<RegistrationHandler> logger)
        {
            _logger = logger;
        }

        [InProcessHandler]
        public RegistrationStatus CreateNewCustomer(RegisterCustomerCommand command)
        {
            _logger.LogDebug("Registration Handler Called");

            // TODO:  Code calling Repositories, Domain Entity, and Services...

            return new RegistrationStatus {
                IsSuccess = command.FirstName != "Vern",
                ReferenceNumber = Guid.NewGuid().ToString(),
                DateAccountActive = DateTime.UtcNow.AddDays(2)
            };
        }

    }
}
