using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Core;

/// <summary>
/// Contract for a component responsible for building an action documentation
/// model from a WebApi action method metadata.
/// </summary>
public interface IDocBuilder
{
    /// <summary>
    /// Creates an action document from associated metadata.
    /// </summary>
    /// <param name="actionMeta">The metadata describing the action method.</param>
    /// <returns>The constructed model.</returns>
    ApiActionDoc BuildActionDoc(ApiActionMeta actionMeta);
}