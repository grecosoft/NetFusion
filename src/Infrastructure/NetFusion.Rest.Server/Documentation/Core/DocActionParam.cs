using NetFusion.Rest.Server.Mappings;
using System;
using System.Xml.Linq;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for a controller action parameter.
    /// </summary>
    public class DocActionParam
    {
        public DocActionParam(XElement element)
        {
            Name = element.Attribute("name")?.Value;
            Description = element.Attribute("description")?.Value;
        }

        public DocActionParam(string name, Type type)
        {
            Name = name;
            Type = type.GetJsTypeName();
        }

        public string Name { get; }
        public string Type { get; }
        public string Description { get; set; }
    }
}
