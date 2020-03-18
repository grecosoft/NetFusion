using NetFusion.Rest.Resources;

namespace TestClasses.ClientRequests.Server
{
    /// <summary>
    /// Server side resource returned by the API Controller under-test.
    /// </summary>
    [ExposedName("cust-address")]
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
