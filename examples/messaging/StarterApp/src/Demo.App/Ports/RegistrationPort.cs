using System;
using Demo.Api.Commands;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging;

namespace Demo.App.Ports
{
    public class RegistrationPort : IMessageConsumer
    {
        private readonly ILogger _logger;

        public RegistrationPort(ILogger<RegistrationPort> logger)
        {
            _logger = logger;
        }

        [InProcessHandler]
        public RegistrationStatus CreateNewCustomer(RegisterCustomerCommand command)
        {
            _logger.LogDebug("Registration Port Called");

            // TODO:  Code calling Repositories, Domain Entity, and Services...

            return new RegistrationStatus {
                IsSuccess = command.FirstName != "Vern",
                ReferenceNumber = Guid.NewGuid().ToString(),
                DateAccountActive = DateTime.UtcNow.AddDays(2)
            };
        }

    }
}