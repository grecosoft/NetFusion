using NetFusion.Bootstrap.Plugins;
using System;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Provides interface for obtaining information about the composite application.
    /// </summary>
    public interface IComposite
    {
        /// <summary>
        /// Reference to the entire composite application built by the application container.
        /// </summary>
        CompositeApplication Application { get; }

        /// <summary>
        /// For a given type, determines the plug-in where it is defined.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <returns>The plug-in containing the type.</returns>
        Plugin GetPluginContainingType(Type type);

        /// <summary>
        /// For a given type name, excluding the assembly name, determines the plug-in where it is defined.
        /// </summary>
        /// <param name="fullTypeName">The type name to search.</param>
        /// <returns>The plug-in containing the type.</returns>
        Plugin GetPluginContainingFullTypeName(string fullTypeName);
    }
}
