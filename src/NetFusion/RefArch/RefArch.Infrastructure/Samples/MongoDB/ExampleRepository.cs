using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RefArch.Api.Models;
using RefArch.Domain.Samples.MongoDb;
using NetFusion.MongoDB;
using MongoDB.Driver;

namespace RefArch.Infrastructure.Samples.MongoDB
{
    public class ExampleRepository : IExampleRepository
    {
        private readonly IMongoDbClient<NetFusionDB> _refArchDb;
        private readonly IMongoCollection<Customer> _customerColl;

        public ExampleRepository(IMongoDbClient<NetFusionDB> refArchDb)
        {
            _refArchDb = refArchDb;
            _customerColl = _refArchDb.GetCollection<Customer>();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _customerColl.InsertOneAsync(customer);
        }

        public async Task<IEnumerable<Customer>> ListCustomersAsync()
        {
            return await _customerColl.Find(_ => true).ToListAsync();
        }
    }
}
