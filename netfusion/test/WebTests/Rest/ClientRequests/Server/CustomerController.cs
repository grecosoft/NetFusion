using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Resources;

namespace WebTests.Rest.ClientRequests.Server
{
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        [HttpGet("{id}")]
        public HalResource GetCustomer(string id)
        {
            var customer = new CustomerModel
            {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = "Mark",
                LastName = "Twain"
            };
            
            return HalResource.ForModel(customer);
        }

        [HttpGet("embedded/resource")]
        public HalResource GetEmbeddedAddress(string id)
        {
            var customer = new CustomerModel
            {
                CustomerId = id,
                FirstName = "Mark",
                LastName = "Twain"
            };

            var address = new AddressModel
            {
                AddressId = Guid.NewGuid().ToString(),
                Street = "111 West Hill Drive",
                City = "Chapel Hill",
                State = "NC",
                ZipCode = "27517"
                
            };

            var resource = HalResource.ForModel(customer);
            resource.Embed(HalResource.ForModel(address), "primary-address");
            return resource;
        }
        
        [HttpGet("embedded/collection")]
        public HalResource GetEmbeddedAddresses(string id)
        {
            var customer = new CustomerModel
            {
                CustomerId = id,
                FirstName = "Mark",
                LastName = "Twain"
            };

            var firstAddress = new AddressModel
            {
                AddressId = Guid.NewGuid().ToString(),
                Street = "111 West Hill Drive",
                City = "Chapel Hill",
                State = "NC",
                ZipCode = "27517"
            };
            
            var secondAddress = new AddressModel
            {
                AddressId = Guid.NewGuid().ToString(),
                Street = "55 Linux Drive",
                City = "DurhamNd",
                State = "NC",
                ZipCode = "27517"
            };

            var resource = HalResource.ForModel(customer);
            resource.Embed(new[]
            {
                HalResource.ForModel(firstAddress), 
                HalResource.ForModel(secondAddress)
            },  "addresses");
            
            return resource;
        }
    }
}
