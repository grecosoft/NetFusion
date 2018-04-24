using System.Xml.Linq;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for an embedded resource.  Provides a description of the key
    /// value used to reference the embedded resource.
    /// </summary>
    public class DocEmbeddedResource
    {
        public DocEmbeddedResource(XElement element)
        {
            EmbeddedName = element.Attribute("name")?.Value;
            Description = element.Attribute("description")?.Value;
        }

        public DocEmbeddedResource(string name, string description)
        {
            EmbeddedName = name;
            Description = description;
        }

        public string EmbeddedName { get; }
        public string Description { get; set; }
    }
}
