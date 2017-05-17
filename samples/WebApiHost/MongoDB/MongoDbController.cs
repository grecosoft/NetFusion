using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using NetFusion.Web.Mvc.Metadata;
using WebApiHost.MongoDB.Models;

namespace WebApiHost.MongoDB
{
    [Route("api/[controller]")]
    [GroupMeta]
    public class MongoDbController : Controller
    {
        private readonly ICustomerRepository _repository;

        public MongoDbController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("customer", Name = "CreateCustomer"), ActionMeta("CreateCustomer")]
        public async Task<CustomerModel> CreateCustomer([FromBody]CustomerModel customer)
        {
            await _repository.AddCustomerAsync(customer);
            return customer;
        }

        [HttpGet("customer", Name = "GetCustomers"), ActionMeta("GetCustomers")]
        public Task<CustomerModel[]> GetCustomers()
        {
            return _repository.ListCustomersAsync();
        }
    }
}
