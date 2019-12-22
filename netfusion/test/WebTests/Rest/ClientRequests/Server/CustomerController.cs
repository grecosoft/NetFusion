using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Hal;
using WebTests.Rest.Setup;

namespace WebTests.Rest.ClientRequests.Server
{
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        private readonly IMockedService _mockedService;
        private readonly IHalEmbeddedResourceContext _resourceContext;

        public CustomerController(
            IMockedService mockedService,
            IHalEmbeddedResourceContext resourceContext)
        {
            _mockedService = mockedService;
            _resourceContext = resourceContext;
        }
        
        [HttpGet("{id}")]
        public HalResource GetCustomer(string id)
        {
            var model = _mockedService.Customers.First();
            return new HalResource(model);
        }

        [HttpGet("embedded/resource")]
        public HalResource GetEmbeddedCustomer(string id)
        {
            var customer = new CustomerModel
            {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = "Mark",
                LastName = "Twain"
            };

            var address = new AddressModel
            {
                Street = "111 West Hill Drive",
                City = "Chapel Hill",
                State = "NC",
                ZipCode = "27517"
                
            };

            var resource = new HalResource(customer);
            resource.Embed(new HalResource(address), "primary-address");
            return resource;
        }
    }
}
