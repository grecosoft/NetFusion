using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Core.Descriptions;

/// <summary>
/// Interface implemented by a class responsible for describing
/// a controller's action method.
/// </summary>
public interface IActionDescription : IDocDescription
{
    /// <summary>
    /// Called to add descriptions to the specified action document.
    /// </summary>
    /// <param name="actionDoc">The action document to describe.</param>
    /// <param name="actionMeta">The associated action metadata used
    /// to query related information.</param>
    void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta);
}