using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Represents a link containing an URL template with tokens to be replaced by the calling client.
    /// </summary>
    public class TemplateUrlLink : ResourceLink
    {
        /// <summary>
        /// The runtime information associated with a selected controller's action.
        /// </summary>
        public MethodInfo ActionMethodInfo { get; internal set; }
    }
}
