using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("pass-through")]
        public CustomerResource GetPassThrough()
        {
            return new CustomerResource();
        }       

        [HttpPost("pass-through")]
        public CustomerResource PostPassThrough([FromBody]CustomerResource resource)
        {
            _mockedService.ServerReceivedResource = resource;
            return new CustomerResource();
        }

        [HttpGet("{id}")]
        public CustomerResource GetCustomer(string id)
        {
            return _mockedService.Customers.First();
        }

        [HttpGet("embedded/resource")]
        public CustomerResource GetEmbeddedCustomer(string id)
        {
            if (_resourceContext.RequestedEmbeddedResources.Length == 0)
            {
                return _mockedService.Customers.First();
            }

            var customer = _mockedService.Customers.First();
            var resource =  new CustomerResource
            {
                CustomerId = customer.CustomerId,
                Age = customer.Age,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
            };

            foreach (var embeddedResource in customer.Embedded)
            {
                if (_resourceContext.RequestedEmbeddedResources.Contains(embeddedResource.Key))
                {
                    var halResource = embeddedResource.Value as IHalResource;
                    if (halResource != null)
                    {

                    }
                    resource.Embed(halResource, embeddedResource.Key);
                }
            }

            return resource;
        }
    }
}
