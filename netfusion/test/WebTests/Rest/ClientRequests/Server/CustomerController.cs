using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;

namespace WebTests.Rest.ClientRequests.Server
{
    [ApiController, Route("api/customers")]
    public class CustomerController : ControllerBase
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

            return customer.AsResource();
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

            var resource = customer.AsResource();
            resource.EmbedModel(address.AsResource(), "primary-address");
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

            var resource = customer.AsResource();
            resource.EmbedModel(new[]
            {
                firstAddress.AsResource(), 
                secondAddress.AsResource()
            },  "addresses");
            
            return resource;
        }
    }
}
