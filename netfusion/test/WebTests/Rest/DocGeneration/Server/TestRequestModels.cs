using NetFusion.Rest.Resources;

namespace WebTests.Rest.DocGeneration.Server
{
    /// <summary>
    /// Example mode populated from the body of a request.
    /// </summary>
    [Resource("TestRequest")]
    public class TestRequestModel
    {
        /// <summary>
        /// The first name read from the request body.
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// The last name read from the request body.
        /// </summary>
        public string LastName { get; set; }
    }
}