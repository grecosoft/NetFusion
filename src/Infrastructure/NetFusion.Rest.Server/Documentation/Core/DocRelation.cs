using NetFusion.Rest.Server.Actions;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for a resource's associated relations.
    /// </summary>
    public class DocRelation
    {
        public string RelName { get; }
        public string Description { get; set; }

        public DocRelation(XElement element)
        {
            RelName = element.Attribute("relName")?.Value;
            Description = element.Attribute("description")?.Value;
        }

        // Populates the documentation from cached action link data.
        public DocRelation(ActionLink actionLink)
        {
            RelName = actionLink.RelationName;

            HrefLang = actionLink.HrefLang;
            Name = actionLink.Name;
            Title = actionLink.Title;
            Type = actionLink.Type;
            IsDepricated = actionLink.Deprecation != null ? true : (bool?)null;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HrefLang { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDepricated { get; }
    }
}
