using System;

namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Used to specify additional metadata about a web-api route.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RouteMetadataAttribute : Attribute
    {
        /// <summary>
        /// Indicates if metadata for the route should be returned to the client.
        /// By default, this value is True.
        /// </summary>
        public bool IncludeRoute { get; set; } = true;
    }
}
