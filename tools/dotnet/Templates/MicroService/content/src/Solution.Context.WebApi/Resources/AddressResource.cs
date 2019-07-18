using System;
using NetFusion.Rest.Resources.Hal;
using Newtonsoft.Json;
using Solution.Context.Domain.Entities;

namespace Solution.Context.WebApi.Resources
{
    public class AddressResource : HalResource
    {
        public Guid Id { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }

        public static AddressResource FromEntity(Address entity)
        {
            return new AddressResource{
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                Address = $"{entity.AddressLine1} {entity.AddressLine2}",
                City = entity.City,
                State = entity.State,
                ZipCode = entity.ZipCode
            };
        }
        
        [JsonIgnore]
        public Guid CustomerId { get; private set; }
    }
}