using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiResponseDoc
    {
        public string Description { get; }
        public ApiResponseMeta Meta { get; }
        
        // The description will be the description of the returns element.
        // Will also want to use comment tags for each property on the response type.

        public ApiResponseDoc(string description, ApiResponseMeta meta)
        {
            Description = description;
            Meta = meta;
        }
    }
}