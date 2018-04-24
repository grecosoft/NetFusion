using NetFusion.Rest.Resources;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Defines a link with an associated relation name.  The relation
    /// name indicates how the link is associated to the resource.
    /// </summary>
    public class ActionLink
    {
        internal string RelationName { get; set; }       
        internal string Href { get; set; }
        internal string HrefLang { get; set; }
        internal IEnumerable<string> Methods { get; set; }

        internal string Name { get; set; }
        internal string Title { get; set; }
        internal string Type { get; set; }

        internal ActionLink Deprecation { get; set; }
        internal ActionLink Profile { get; set; }

        // Determine if the link can be applied to another resource type.  
        internal virtual bool CanBeAppliedTo(Type resourceType)
        {
            return true;
        }

        // To reduce code duplication, a new link defined for one resource type can be applied
        // to another resource type.  This is common with resource view models.  The view model
        // represents the same resource but a different view of the information.
        internal virtual void CopyTo<TNewResourceType>(ActionLink actionLink)
            where TNewResourceType: class, IResource
        {
            actionLink.RelationName = RelationName;
            actionLink.Href = Href;
            actionLink.HrefLang = HrefLang;
            actionLink.Methods = Methods;
            actionLink.Name = Name;
            actionLink.Title = Title;
            actionLink.Type = Type;
            actionLink.Deprecation = Deprecation?.CreateCopyFor<TNewResourceType>();
            actionLink.Profile = Profile?.CreateCopyFor<TNewResourceType>();
        }

        // Create a new instance of the link for a different resource type and copies
        // all link metadata for original.
        internal virtual ActionLink CreateCopyFor<TNewResourceType>()
            where TNewResourceType : class, IResource
        {
            var newResourceLink = new ActionLink();
            CopyTo<TNewResourceType>(newResourceLink);
            return newResourceLink;
        }
    }
}