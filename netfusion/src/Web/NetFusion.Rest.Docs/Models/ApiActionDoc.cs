using System.Collections.Generic;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiActionDoc
    {
        public string Description { get; }
        public string RelativePath { get; }
        public string HttpMethod { get; }
        
        public ApiActionDoc(string description, string relativePath, string httpMethod)
        {
            
            Description = description;
            RelativePath = relativePath;
            HttpMethod = httpMethod;
        }
        
        public ICollection<ApiParameterDoc> RouteParams { get; } = new List<ApiParameterDoc>();
        public ICollection<ApiParameterDoc> QueryParams { get; } = new List<ApiParameterDoc>();
        public ICollection<ApiParameterDoc> HeaderParams { get; } = new List<ApiParameterDoc>();
        
        public ApiEmbeddedDoc[] EmbeddedResources { get; set; }
        public ApiResponseMeta Response { get; set; }
    }
}