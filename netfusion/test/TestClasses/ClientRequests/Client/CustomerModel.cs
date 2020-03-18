namespace TestClasses.ClientRequests.Client
{
    /// <summary>
    /// Client side resource class modeling the corresponding
    /// server side returned resource.
    /// </summary>
    public class CustomerModel 
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
