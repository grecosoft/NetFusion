using NetFusion.WebApi.Metadata;
using RefArch.Api.Models;
using RefArch.Domain.Samples.MongoDb;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.MongoDB", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/mongodb")]
    public class MongoDBController : ApiController
    {
        private readonly IExampleRepository _repository;

        public MongoDBController(IExampleRepository repository)
        {
            _repository = repository;
        }

        [HttpPost, Route("customer", Name = "CreateCustomer")]
        public async Task<Customer> CreateCustomer(Customer customer)
        {
            await _repository.AddCustomerAsync(customer);
            return customer;
        }

        [HttpGet, Route("customer", Name = "GetCustomers")]
        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            return await _repository.ListCustomersAsync();
        }
    }
}