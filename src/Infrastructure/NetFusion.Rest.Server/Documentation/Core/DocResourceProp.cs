using NetFusion.Rest.Server.Mappings;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for a specific resource property.
    /// </summary>
    public class DocResourceProp
    {
        public string Name { get; }    
        public string Type { get; }

        public DocResourceProp(PropertyInfo propInfo)
        {
            Name = propInfo.Name;
            Type = propInfo.PropertyType.GetJsTypeName();
        }

        public string Description { get; set; }
    }
}
