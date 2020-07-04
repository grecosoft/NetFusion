using System.Collections.Generic;
using NetFusion.Rest.Docs.Core.Description;

namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model representing documentation for a specific
    /// Web Api method.
    /// </summary>
    public class ApiActionDoc
    {
        /// <summary>
        /// The relative path based on the host.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// The method used to invoke the action.
        /// </summary>
        public string HttpMethod { get; }
        
        public ApiActionDoc(string relativePath, string httpMethod)
        {
            RelativePath = relativePath;
            HttpMethod = httpMethod;
        }

        /// <summary>
        /// Description of the functionality implemented by the action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description of the route parameters accepted by the action method.
        /// </summary>
        public ICollection<ApiParameterDoc> RouteParams { get; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Description of the query string parameters accepted by the action method.
        /// </summary>
        public ICollection<ApiParameterDoc> QueryParams { get; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Description of the header parameters accepted by the action method.
        /// </summary>
        public ICollection<ApiParameterDoc> HeaderParams { get; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Contains documention for each of the possible HTTP status codes and the
        /// associated resource returned for the status.
        /// </summary>
        public ICollection<ApiResponseDoc> ResponseDocs { get; } = new List<ApiResponseDoc>();

        internal EmbeddedType[] EmbeddedTypes { get; set; }
    }
}