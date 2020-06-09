using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Represents a link containing an URL template with tokens to be replaced by the calling client.
    /// </summary>
    public class TemplateUrlLink : ResourceLink
    {
        /// <summary>
        /// The group name specified on a controller using the GroupMeta attribute.
        /// </summary>
        public string GroupTemplateName { get; internal set; }

        /// <summary>
        /// The action name specified on a controller's method using the ActionMeta attribute.
        /// </summary>
        public string ActionTemplateName { get; internal set; }
        
        /// <summary>
        /// The runtime information associated with a selected controller's action.
        /// </summary>
        public MethodInfo ActionMethodInfo { get; internal set; }
    }
}
