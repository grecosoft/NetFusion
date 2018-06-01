using Newtonsoft.Json;

namespace NetFusion.Web.Mvc.Metadata.Models
{
    public class ActionInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("parameters")]
        public ParameterInfo[] Parameters { get; set; }
    }
}