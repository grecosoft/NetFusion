using System.Threading.Tasks;
using NetFusion.Messaging;
using Solution.Context.Domain.Commands;
using Solution.Context.Domain.Entities;
using Solution.Context.Domain.Events;
using Solution.Context.Domain.Queries;
using Solution.Context.Domain.Repositories;

namespace Solution.Context.App.Handlers
{
    public class CustomerHandler : IMessageConsumer,
        IQueryConsumer
    {
        private readonly IMessagingService _messaging;
        private readonly ICustomerRepository _customerRepo;
        private readonly ISuggestionRepository _suggestionRepo;

        public CustomerHandler(IMessagingService messaging,
            ICustomerRepository customerRepo,
            ISuggestionRepository suggestionRepo)
        {
            _messaging = messaging;
            _customerRepo = customerRepo;
            _suggestionRepo = suggestionRepo;
        }

        [InProcessHandler]
        public async Task<Customer> OnRegistration(RegistrationCommand command)
        {
            // Create domain entities from the command and associate them.
            var customer = Customer.Register(command.Prefix, 
                command.FirstName, 
                command.LastName, 
                command.Age);

            var address = Address.Define(customer.Id, command.AddressLine1, command.AddressLine2, 
                command.City, 
                command.State, 
                command.ZipCode);
            
            customer.AssignAddress(address);

            // Save the domain entities to the repository.
            await _customerRepo.SaveAsync(customer);

            // Notify other application components that a new registration was created.
            var domainEvent = new NewRegistrationEvent(customer);
            await _messaging.PublishAsync(domainEvent);

            return customer;
        }

        [InProcessHandler]
        public async Task<CustomerInfo> FindCustomer(QueryCustomer query)
        {
            Customer customer = await  _customerRepo.ReadAsync(query.Id);
            if (customer == null)
            {
                return null;
            }

            Address primaryAddress = customer.GetPrimaryAddress();

            var info = new CustomerInfo
            {
                Customer = customer
            };

            if (query.IncludedSuggestions && primaryAddress != null)
            {
                info.Suggestions = await _suggestionRepo.LookupSuggestedAutos(primaryAddress.State, customer.Age);
            }

            return info;
        }
    }
}