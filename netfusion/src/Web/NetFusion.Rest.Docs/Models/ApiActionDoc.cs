using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model representing documentation for a specific Web Api method.
    /// </summary>
    public class ApiActionDoc
    {
        // Required for deserialization if consumed by .Net Client such
        // as the unit-tests.  The client will most often be JS based.
        public ApiActionDoc() { }

        /// <summary>
        /// The relative path used to invoke the action.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// The method used to invoke the action.
        /// </summary>
        public string HttpMethod { get; set; }
        
        public ApiActionDoc(string relativePath, string httpMethod)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new System.ArgumentException("Value must be specified.", nameof(relativePath));
            }

            if (string.IsNullOrWhiteSpace(httpMethod))
            {
                throw new System.ArgumentException("Value must be specified.", nameof(httpMethod));
            }

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
        public ICollection<ApiParameterDoc> RouteParams { get; set; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Description of the query string parameters accepted by the action method.
        /// </summary>
        public ICollection<ApiParameterDoc> QueryParams { get; set; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Description of the header parameters accepted by the action method.
        /// </summary>
        public ICollection<ApiParameterDoc> HeaderParams { get; set; } = new List<ApiParameterDoc>();
        
        /// <summary>
        /// Description of the parameters populated from the message body.
        /// </summary>
        public ICollection<ApiParameterDoc> BodyParams { get; set; } = new List<ApiParameterDoc>();

        /// <summary>
        /// Contains documentation for each of the possible HTTP status codes and the
        /// associated optional resource returned for the status.
        /// </summary>
        public ICollection<ApiResponseDoc> ResponseDocs { get; set; } = new List<ApiResponseDoc>();

        internal EmbeddedResourceAttribute[] EmbeddedResourceAttribs { get; set; }
    }
}