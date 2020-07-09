namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model representing the possible status code returned from a
    /// Web Api method and the optional associated response resource.
    /// </summary>
    public class ApiResponseDoc
    {
        /// <summary>
        /// The possible set of HTTP response status code.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// The documentation associated with the resource that will
        /// be returned for the status code.  This property will be
        /// null if a resource is not returned for the status code.
        /// </summary>
        public ApiResourceDoc ResourceDoc { get; set; } 
    }
}