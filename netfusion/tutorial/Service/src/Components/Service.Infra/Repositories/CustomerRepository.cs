namespace Service.Infra.Repositories
{
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using Service.Domain.Entities;
    using Service.Domain.Repositories;
    using Service.Infra.Databases;
    using NetFusion.MongoDB;

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IMongoDbClient<ContactDb> _contactDb;
        private readonly IMongoCollection<Customer> _customerColl;
        
        public CustomerRepository(
            IMongoDbClient<ContactDb> contactDb)
        {
            _contactDb = contactDb;
            _customerColl = _contactDb.GetCollection<Customer>();
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