using System;
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
            _mockedService.ServerReceivedResource = new HalResource(resource);
            return new CustomerResource();
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
//            if (_resourceContext.RequestedEmbeddedResources.Length == 0)
//            {
//                return new HalResource(_mockedService.Customers.First());
//            }
//
//            var customer = _mockedService.Customers.First();
//            var resource =  new CustomerResource
//            {
//                CustomerId = customer.CustomerId,
//                Age = customer.Age,
//                FirstName = customer.FirstName,
//                LastName = customer.LastName,
//            };
//
//            foreach (var embeddedResource in customer.Embedded)
//            {
//                if (_resourceContext.RequestedEmbeddedResources.Contains(embeddedResource.Key))
//                {
//                    var halResource = embeddedResource.Value as IHalResource;
//                    if (halResource != null)
//                    {
//
//                    }
//                    resource.Embed(halResource, embeddedResource.Key);
//                }
//            }
//
//            return resource;
                throw new NotImplementedException();
        }
    }
}
