using System;
using System.Linq;
using NetFusion.Web.Rest.Docs.Core.Descriptions;
using NetFusion.Web.Rest.Docs.Models;
using NetFusion.Web.Rest.Docs.Plugin;
using NetFusion.Web.Rest.Server.Linking;

namespace NetFusion.Web.Rest.Docs.Json.Descriptions;

/// <summary>
/// Called to associate additional documentation with a resource relation document.
/// </summary>
public class JsonHalRelationComments : IRelationDescription
{
    private readonly IDocModule _docModule;
        
    public JsonHalRelationComments(IDocModule docModule)
    {
        _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));
    }
        
    public void Describe(ApiResourceDoc resourceDoc, ApiRelationDoc relationDoc, ResourceLink resourceLink)
    {
        // Check if comments exist for the specific resource and relation name.
        var relationComment = _docModule.HalComments.RelationComments.FirstOrDefault(r =>
            r.ResourceName == resourceDoc.ResourceName &&
            r.RelationName == relationDoc.Name);

        if (relationComment != null)
        {
            relationDoc.Description = relationComment.Comments;
            return;
        }
            
        // Check if comments are specified for just the relation name.
        relationComment = _docModule.HalComments.RelationComments
            .FirstOrDefault(r => r.RelationName == relationDoc.Name);

        // If no comments are found, use the default comments if present.
        relationDoc.Description = relationComment?.Comments ?? relationDoc.Description;
    }
}