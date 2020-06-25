using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiResourceDoc
    {
        // Add links descriptions? ...
        public string Description { get; set; }
        public string ResourceName { get; set; } 
        public ICollection<ApiPropertyDoc> Properties { get; } = new List<ApiPropertyDoc>();
    }
}