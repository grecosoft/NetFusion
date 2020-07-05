using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model representing the possible status codes returned from a
    /// Web Api method and their optional associated response resources.
    /// </summary>
    public class ApiResponseDoc
    {
        /// <summary>
        /// The possible set of HTTP response status codes.
        /// </summary>
        public int[] Statuses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<ApiResourceDoc> ResourceDocs { get; } = new List<ApiResourceDoc>();       
    }
}