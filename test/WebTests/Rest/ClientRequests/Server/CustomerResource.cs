using NetFusion.Rest.Resources.Hal;

namespace InfrastructureTests.Web.Rest.ClientRequests.Server
{
    /// <summary>
    /// Server side resource returned by the API Controller under-test.
    /// </summary>
    public class CustomerResource : HalResource
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
