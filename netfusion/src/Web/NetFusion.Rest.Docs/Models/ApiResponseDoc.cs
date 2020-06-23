using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiResponseDoc
    {
        public string Description { get; set; }
        public int[] Statuses { get; set; }
        public ICollection<ApiResourceDoc> ResourceDocs { get; } = new List<ApiResourceDoc>();


        // The description will be the description of the returns element.
        // Will also want to use comment tags for each property on the response type.

       
    }
}