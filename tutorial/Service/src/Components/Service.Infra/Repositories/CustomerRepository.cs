using System.Threading.Tasks;
using MongoDB.Driver;
using NetFusion.MongoDB;
using Service.Domain.Entities;
using Service.Domain.Repositories;
using Service.Infra.Databases;

namespace Service.Infra.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IMongoCollection<Customer> _customerColl;
        
        public CustomerRepository(
            IMongoDbClient<ContactDb> contactDb)
        {
            _customerColl = contactDb.GetCollection<Customer>();
        }
        
        public Task AddCustomerAsync(Customer customer)
        {
            return _customerColl.InsertOneAsync(customer);
        }

        public async Task<Customer> ReadCustomerAsync(string customerId)
        {
            var results = await _customerColl.FindAsync(c => c.CustomerId == customerId);
            return await results.FirstOrDefaultAsync();
        }

        public Task UpdateCustomerAsync(Customer customer)
        {
            return _customerColl.ReplaceOneAsync(c => c.CustomerId == customer.CustomerId, customer);
        }
    }
}