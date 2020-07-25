using System;
using NetFusion.Rest.Resources;

namespace Service.WebApi.Resources
{
    [Resource("type-address")]
    public class AddressResource 
    {
        public Guid Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}