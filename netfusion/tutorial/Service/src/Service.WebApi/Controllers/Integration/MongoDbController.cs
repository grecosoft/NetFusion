using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Entities;
using Service.Domain.Repositories;

namespace Service.WebApi.Controllers.Integration
{
    [Route("api/integration/mongodb"),
     GroupMeta(nameof(MongoDbController))]
    public class MongoDbController : Controller
    {
        private readonly ICustomerRepository _customerRepo;
        
        public MongoDbController(
            ICustomerRepository customerRepo)
        {
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
        }
        
        [HttpPost("customers"), ActionMeta(nameof(AddCustomer))]
        public async Task<IActionResult> AddCustomer([FromBody]Customer customer)
        {
            await _customerRepo.AddCustomerAsync(customer);
            return Ok(customer);
        }

        [HttpGet("customers/{customerId}"), ActionMeta(nameof(ReadCustomer))]
        public async Task<IActionResult> ReadCustomer(string customerId)
        {
            var customer = await _customerRepo.ReadCustomerAsync(customerId);
            return Ok(customer);
        }
        
        [HttpPost("customers/{customerId}"), ActionMeta(nameof(UpdateCustomer))]
        public async Task<IActionResult> UpdateCustomer(string customerId, [FromBody]Customer customer)
        {
            customer.CustomerId = customerId;
            
            await _customerRepo.UpdateCustomerAsync(customer);
            return Ok(customer);
        }
    }
}