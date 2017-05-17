using System;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// This interface is used by the NetFusion.Test assembly 
    /// and is implemented by classes representing mock plug-ins.
    /// This interface is used so a plug-in can add its needed 
    /// types for unit-test purposes without having a dependency 
    /// on the NetFusion.Test assembly.
    /// </summary>
    public interface IPluginTypeAccessor
    {
        /// <summary>
        /// Adds types that are to be associated with the plug-in.
        /// </summary>
        /// <param name="types">The list of types.</param>
        /// <returns>Reference to self.</returns>
        IPluginTypeAccessor AddPluginType(params Type[] types);

        /// <summary>
        /// Adds a type that is to be associated with the plug-in.
        /// </summary>
        /// <typeparam name="T">The type to be added.</typeparam>
        /// <returns>Reference to self.</returns>
        IPluginTypeAccessor AddPluginType<T>();
    }
}
