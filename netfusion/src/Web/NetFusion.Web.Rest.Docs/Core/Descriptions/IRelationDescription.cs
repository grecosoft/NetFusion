using NetFusion.Web.Rest.Docs.Models;
using NetFusion.Web.Rest.Server.Linking;

namespace NetFusion.Web.Rest.Docs.Core.Descriptions;

/// <summary>
/// Interface implemented by a class responsible for describing
/// a relation associated with a resource.
/// </summary>
public interface IRelationDescription : IDocDescription
{
    /// <summary>
    /// Called to add documentation to a resource's relation.
    /// </summary>
    /// <param name="resourceDoc">The resource for which the relation is associated.</param>
    /// <param name="relationDoc">The created relation document model.</param>
    /// <param name="resourceLink">The link associated with the relation.</param>
    void Describe(ApiResourceDoc resourceDoc, ApiRelationDoc relationDoc, ResourceLink resourceLink);
}