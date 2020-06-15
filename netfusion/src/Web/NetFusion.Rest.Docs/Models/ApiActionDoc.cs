using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiActionDoc
    {
        public string Description { get; }
        public ApiActionMeta Meta { get; }

        public ApiActionDoc(string description, ApiActionMeta meta)
        {
            Description = description;
            Meta = meta;
        }
        
        public ApiParameterDoc[] RouteParameters { get; set; } 
        public ApiParameterDoc[] QueryParameters { get; set; } 
        public ApiParameterMeta[] HeaderParams { get; set; }
        
        public ApiEmbeddedDoc[] EmbeddedResources { get; set; }
        public ApiResponseMeta Response { get; set; }
    }
}