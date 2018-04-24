using Newtonsoft.Json;
using System.Collections.Generic;

namespace NetFusion.Web.Mvc.Metadata.Models
{
    public class GroupInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("actions")]
        public IDictionary<string, ActionInfo> Actions { get; set; }
    }
}