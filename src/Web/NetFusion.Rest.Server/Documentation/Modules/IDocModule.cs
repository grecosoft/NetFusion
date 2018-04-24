using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Documentation.Core;
using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Modules
{
    /// <summary>
    /// Contains API methods exposed by the documentation module.
    /// </summary>
    public interface IDocModule : IPluginModuleService
    {
        /// <summary>
        /// Returns documentation for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The controller's action method.</param>
        /// <returns>Action documentation.  If no documentation is available,
        /// a null reference is returned.</returns>
        DocAction GetActionDoc(MethodInfo actionMethod);

        /// <summary>
        /// Returns the common definition documentation shared across actions.
        /// This can contain common status-code and relation definitions used
        /// by several controller actions.
        /// </summary>
        /// <returns>Common action documentation.</returns>
        CommonDefinitions GetCommonDefinitions();

        /// <summary>
        /// Returns the documentation for a resource type containing
        /// information about a resource's properties.
        /// </summary>
        /// <param name="resourceType">The type of the resource.</param>
        /// <returns>The resource documentation.</returns>
        DocResource GetResourceDoc(Type resourceType);
    }
}
