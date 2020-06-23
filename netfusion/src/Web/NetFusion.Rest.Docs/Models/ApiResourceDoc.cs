using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiResourceDoc
    {
        public string Name { get; set; } // ??
        public string Description { get; set; }
        public ICollection<ApiPropertyDoc> Properties { get; } = new List<ApiPropertyDoc>();
    }
}