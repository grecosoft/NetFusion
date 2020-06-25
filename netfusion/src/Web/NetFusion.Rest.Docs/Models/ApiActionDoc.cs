using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiActionDoc
    {
        public string RelativePath { get; }
        public string HttpMethod { get; }
        
        public ApiActionDoc(string relativePath, string httpMethod)
        {
            RelativePath = relativePath;
            HttpMethod = httpMethod;
        }
        
        public string Description { get; set; }
        
        public ICollection<ApiParameterDoc> RouteParams { get; } = new List<ApiParameterDoc>();
        public ICollection<ApiParameterDoc> QueryParams { get; } = new List<ApiParameterDoc>();
        public ICollection<ApiParameterDoc> HeaderParams { get; } = new List<ApiParameterDoc>();
        
        public ApiEmbeddedDoc[] EmbeddedResources { get; set; }
        public ICollection<ApiResponseDoc> ResponseDocs { get; } = new List<ApiResponseDoc>();
        
        internal Type[] EmbeddedTypes { get; set; }
    }
}