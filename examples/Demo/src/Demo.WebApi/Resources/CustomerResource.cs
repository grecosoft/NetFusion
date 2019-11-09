using System;
using System.Linq;
using NetFusion.Rest.Resources.Hal;
using Demo.Domain.Entities;

namespace Demo.WebApi.Resources
{
    public class CustomerResource : HalResource
    {
        public Guid Id { get; private set;}
        public string Name { get; private set;}
        public int Age { get; private set; }
        public bool HasPrimaryAddress { get; private set; }

        public static CustomerResource FromEntity(Customer entity)
        {
            return new CustomerResource
            {
                Id = entity.Id,
                Name = $"{entity.Prefix}, {entity.FirstName} {entity.LastName}",
                Age = entity.Age,
                HasPrimaryAddress = entity.Addresses.Any(a => a.IsPrimaryAddress)
            };
        }

        public static CustomerResource FromEntity(CustomerInfo entity)
        {

            var resource = FromEntity(entity.Customer);

            var addressResources = entity.Customer.Addresses.Select(AddressResource.FromEntity);
            resource.Embed(addressResources, "addresses");

            if (entity.Suggestions != null)
            {
                var suggestionsResource = entity.Suggestions.Select(AutoResource.FromEntity);
                
                resource.Embed(suggestionsResource, "auto-suggestions");
            }
            
            return resource;
        }
    }
}