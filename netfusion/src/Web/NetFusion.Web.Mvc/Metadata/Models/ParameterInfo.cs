using Newtonsoft.Json;

namespace NetFusion.Web.Mvc.Metadata.Models
{
    public class ParameterInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isOptional")]
        public bool IsOptional { get; set; }

        [JsonProperty("defaultValue")]
        public object DefaultValue { get; set; }

        [JsonProperty("type")]
        public object Type { get; set; }
    }
}
