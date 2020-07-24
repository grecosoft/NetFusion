using NetFusion.Rest.Resources;

namespace WebTests.Rest.ClientRequests.Server
{
    /// <summary>
    /// Server side resource returned by the API Controller under-test.
    /// </summary>
    [Resource("cust-address")]
    public class AddressModel 
    {
        public string AddressId { get; set; }
        public string CustomerId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
