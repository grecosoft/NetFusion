using System;
using System.Linq;
using NetFusion.Web.Rest.Docs.Core.Descriptions;
using NetFusion.Web.Rest.Docs.Models;
using NetFusion.Web.Rest.Docs.Plugin;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.Rest.Docs.Json.Descriptions;

/// <summary>
/// Called to associate additional documentation with an embedded resource document.
/// </summary>
public class JsonHalEmbeddedComments(IDocModule docModule) : IEmbeddedDescription
{
    private readonly IDocModule _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));

    public void Describe(ApiEmbeddedDoc embeddedDoc, EmbeddedResourceAttribute attribute)
    {
        // Check for a specific comment defined for the parent and child resources.
        var embeddedComments =_docModule.HalComments.EmbeddedComments.FirstOrDefault(c =>
            c.EmbeddedName == embeddedDoc.EmbeddedName &&
            c.ParentResourceName == attribute.ParentResourceType.GetResourceName() &&
            c.ChildResourceName == attribute.ChildResourceType.GetResourceName());

        if (embeddedComments != null)
        {
            embeddedDoc.Description = embeddedComments.Comments;
            return;
        }
            
        // Check if a comment exists just based on the embedded name.
        embeddedComments = _docModule.HalComments.EmbeddedComments.FirstOrDefault(c =>
            c.EmbeddedName == embeddedDoc.EmbeddedName);

        embeddedDoc.Description = embeddedComments?.Comments ?? embeddedDoc.Description;
    }
}