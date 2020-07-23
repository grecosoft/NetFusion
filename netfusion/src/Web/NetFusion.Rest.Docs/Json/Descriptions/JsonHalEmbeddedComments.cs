using System;
using System.Linq;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Docs.Json.Descriptions
{
    /// <summary>
    /// Called to associate additional documentation with an embedded resource document.
    /// </summary>
    public class JsonHalEmbeddedComments : IEmbeddedDescription
    {
        private readonly IDocModule _docModule;
        
        public JsonHalEmbeddedComments(IDocModule docModule)
        {
            _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));
        }
        
        public void Describe(ApiEmbeddedDoc embeddedDoc, EmbeddedResourceAttribute attribute)
        {
            // Check for a specific comment defined for the parent and child resources.
            var embeddedComments =_docModule.HalComments.EmbeddedComments.FirstOrDefault(c =>
                c.EmbeddedName == embeddedDoc.EmbeddedName &&
                c.ParentResourceName == attribute.ParentResourceType.GetExposedResourceName() &&
                c.ChildResourceName == attribute.ChildResourceType.GetExposedResourceName());

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
}