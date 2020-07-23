using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Docs.Core.Descriptions
{
    /// <summary>
    /// Interface implemented by a class responsible for describing
    /// a relation associated with a resource.
    /// </summary>
    public interface IRelationDescription : IDocDescription
    {
        /// <summary>
        /// Called to add documentation to a resource.
        /// </summary>
        /// <param name="relationDoc">The created relation document model.</param>
        /// <param name="resourceLink">The link associated with the relation.</param>
        void Describe(ApiRelationDoc relationDoc, ResourceLink resourceLink);
    }
}