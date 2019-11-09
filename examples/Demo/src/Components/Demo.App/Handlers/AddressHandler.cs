using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging;
using Demo.Domain.Commands;
using Demo.Domain.Entities;
using Demo.Domain.Events;
using Demo.Domain.Repositories;

namespace Demo.App.Handlers
{
    public class AddressHandler : IMessageConsumer
    {
        private readonly ILogger _logger;
        private readonly ICustomerRepository _customerRepo;

        public AddressHandler(ILogger<AddressHandler> logger,
            ICustomerRepository customerRepo)
        {
            _logger = logger;
            _customerRepo = customerRepo;
        }

        [InProcessHandler]
        public async Task OnSetPrimary(SetPrimaryAddressCommand command)
        {
            Customer customer = await _customerRepo.ReadAsync(command.CustomerId);
            customer.SetPrimaryAddressTo(command.AddressId);
        }

        [InProcessHandler]
        public Task ValidateAddress(NewRegistrationEvent domainEvent)
        {
            _logger.LogDebug("Validate Address...");
            
            // Would typically call asynchronous method to retrieve information
            // required to validation.
            return Task.CompletedTask;
        }
    }}