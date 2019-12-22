namespace WebTests.Rest.ClientRequests.Client
{
    /// <summary>
    /// Client side resource class modeling the corresponding
    /// server side returned resource.
    /// </summary>
    public class AddressModel 
    {
        public string AddressId { get; set; }
        public string CustomerId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
