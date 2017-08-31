using NetFusion.Rest.Client.Resources;

namespace InfrastructureTests.Web.Rest.ClientRequests.Client
{
    /// <summary>
    /// Client side resource class modeling the corresponding
    /// server side returned resource.
    /// </summary>
    public class CustomerModel : HalResource
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
