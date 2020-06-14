using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiParameterDoc
    {
        public string Description { get; }
        public string Type { get; }
        public ApiParameterMeta Meta { get; }

        public ApiParameterDoc(string description, ApiParameterMeta meta)
        {
            Description = description;
            Meta = meta;
        }
    }
}