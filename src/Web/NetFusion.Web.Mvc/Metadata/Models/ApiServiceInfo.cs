using Newtonsoft.Json;
using System.Collections.Generic;

namespace NetFusion.Web.Mvc.Metadata.Models
{
    public class ApiServiceInfo
    {
        [JsonProperty("groups")]
        public IDictionary<string, GroupInfo> Groups { get; set; }
    }
}