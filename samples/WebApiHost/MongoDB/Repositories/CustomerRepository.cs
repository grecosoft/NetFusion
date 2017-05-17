using MongoDB.Driver;
using NetFusion.MongoDB;
using System.Threading.Tasks;
using WebApi.MongoDB;
using WebApiHost.MongoDB.Models;

namespace WebApiHost.MongoDB.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IMongoDbClient<ContactDB> _contactDB;
        private readonly IMongoCollection<CustomerModel> _customerColl;

        public CustomerRepository(IMongoDbClient<ContactDB> contactDB)
        {
            _contactDB = contactDB;
            _customerColl = _contactDB.GetCollection<CustomerModel>();
        }

        public Task AddCustomerAsync(CustomerModel customer)
        {
            return _customerColl.InsertOneAsync(customer);
        }

        public async Task<CustomerModel[]> ListCustomersAsync()
        {
            var results = await _customerColl.Find(_ => true).ToListAsync();
            return results.ToArray();
        }
    }
}
