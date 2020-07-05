using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    /// <summary>
    /// Interface implemented by a class responsible for describing
    /// all resource relations return by a controller's action method.
    /// </summary>
    public interface ILinkedDescription : IDocDescription
    {
        /// <summary>
        /// Call to add descriptions to all resource relations.
        /// </summary>
        /// <param name="actionDoc">The action document containing the
        /// resource relations to describe.</param>
        /// <param name="actionMeta">The associated action metadata used
        /// to query related information.</param>
        void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta);
    }
}
