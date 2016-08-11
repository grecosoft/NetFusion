using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Provides interface for obtaining information about the
    /// composite application.
    /// </summary>
    public interface IComposite
    {
        /// <summary>
        /// The plug-ins from which the composite application was built.
        /// </summary>
        IEnumerable<Plugin> Plugins { get; }

        /// <summary>
        /// For a given type, determines the plug-in where it is defined.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <returns>The plug-in containing the type.</returns>
        Plugin GetPluginForType(Type type);
    }
}
