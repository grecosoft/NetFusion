using System.Reflection;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs;

/// <summary>
/// The main service for returning documentation for a specific WebApi method.
/// </summary>
public interface IApiDocService
{
    /// <summary>
    /// Returns documentation for a WebApi method.
    /// </summary>
    /// <param name="actionMethodInfo">The controller's method.</param>
    /// <param name="actionDoc">The documentation if generated.</param>
    /// <returns>True if documentation could be generated.  Otherwise, False.</returns>
    bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc);

    /// <summary>
    /// Returns documentation for a WebApi method.
    /// </summary>
    /// <param name="httpMethod">The HTTP method associated with the WebApi method.</param>
    /// <param name="relativePath">The URL associated with the WebApi method.</param>
    /// <param name="actionDoc">The documentation if generated.</param>
    /// <returns>True if documentation could be generated.  Otherwise, False.</returns>
    bool TryGetActionDoc(string httpMethod, string relativePath, out ApiActionDoc actionDoc);
}