using System;
using NetFusion.Rest.Resources.Hal;

namespace Service.WebApi.Resources
{
    public class AddressResource : HalResource
    {
        public Guid Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}