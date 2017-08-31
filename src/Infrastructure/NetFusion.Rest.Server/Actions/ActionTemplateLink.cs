using System;

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

        // Populate new instance of the link related with new resource type.
        internal override void CopyTo<TNewResourceType>(ActionLink actionLink)
        {
            base.CopyTo<TNewResourceType>(actionLink);

            var actionUrlLink = (ActionTemplateLink)actionLink;
            actionUrlLink.GroupTemplateName = GroupTemplateName;
            actionUrlLink.ActionTemplateName = ActionTemplateName;
        }

        // Create new instance of the link for the new resource type.
        internal override ActionLink CreateCopyFor<TNewResourceType>()
        {
            var newResourceLink = new ActionTemplateLink();

            CopyTo<TNewResourceType>(newResourceLink);
            return newResourceLink;
        }
    }
}
