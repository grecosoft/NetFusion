using System;

namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Can be specified an a Web Api Controller to change the name that is used
    /// to refer to the controller in the meta-data returned to the client.  
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EndpointMetadataAttribute : Attribute
    {
        /// <summary>
        /// The name of the endpoint in the returned meta-data.
        /// </summary>
        public string EndpointName { get; set; }

        /// <summary>
        /// Determines if all the routes should be included in the returned
        /// endpoint meta-data.  This property is False by default.
        /// </summary>
        public bool IncluedAllRoutes { get; set; }
    }
}
        