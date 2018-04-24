using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;

namespace InfrastructureTests.Web.Rest.ClientRequests.Server
{
    /// <summary>
    /// Server side resource returned by the API Controller under-test.
    /// </summary>
    [NamedResource("cust-address")]
    public class AddressResource : HalResource
    {
        public string AddressId { get; set; }
        public string CustomerId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
