namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model containing types defined within the plugin based on
    /// abstract types defined  by the plugin or other plugins.  
    /// These types are defined to use the functionality provided 
    /// by the plugin defining the abstract type on which it is based.
    /// </summary>
    public class KnownTypeDefinition
    {
        public string DefinitionTypeName { get; set; }
        public string[] DiscoveringPlugins { get; set; }
    }
}
