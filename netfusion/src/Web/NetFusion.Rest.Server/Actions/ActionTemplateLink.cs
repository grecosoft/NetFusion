namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Represents a link containing an URL template with tokens 
    /// to be replaced by the calling client.
    /// </summary>
    public class ActionTemplateLink : ActionLink
    {
        /// <summary>
        /// The group name specified on a controller using the GroupMeta attribute.
        /// </summary>
        public string GroupTemplateName { get; internal set; }

        /// <summary>
        /// The action name specified on a controller's method using the ActionMeta attribute.
        /// </summary>
        public string ActionTemplateName { get; internal set; }
    }
}
