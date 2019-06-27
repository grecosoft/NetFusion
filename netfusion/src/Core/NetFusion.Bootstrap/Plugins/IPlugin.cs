using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Interface representing a plugin used to build a composite container.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Unique value identifying the plugin.  Can be used to obtain the Id of the
        /// Host Plugin used to uniquely identify associated items such as Queues.
        /// </summary>
        string PluginId { get; }
        
        /// <summary>
        /// The type of the plugin used to determine the types from which it can
        /// be composed.  Core plugins are composed from all other registered plugin
        /// types.  Whereas Application centric plugins are only composed from types
        /// contained in other application plugins. 
        /// </summary>
        PluginTypes PluginType { get; }    
        string Name { get; }

        /// <summary>
        /// The name of the .NET assembly containing the plugin.
        /// </summary>
        string AssemblyName { get; set; }
        
        /// <summary>
        /// The version of the .NET assembly containing the plugin.
        /// </summary>
        string AssemblyVersion { get; set; }
        
        /// <summary>
        /// The modules for which the plugin is composed.  Modules are used to
        /// organize the code for a plugin's implementation and are invoked
        /// when the CompositeContainer is built.
        /// </summary>
        IEnumerable<IPluginModule> Modules { get; }
        
        /// <summary>
        /// The configurations defined by the plugin. A plugin's configurations
        /// can be set by the host application or by another plugin to extend
        /// the behaviors of the plugin defining the configuration.
        /// </summary>
        IEnumerable<IPluginConfig> Configs { get; }
        
        /// <summary>
        /// The types defined by the plugin.  These are all types contained within
        /// the assembly of the plugin. 
        /// </summary>
        IEnumerable<Type> Types { get; }
        
        /// <summary>
        /// Optional description of the plugin.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Optional URL referencing the plugin's source code.
        /// </summary>
        string SourceUrl { get; }
        
        /// <summary>
        /// Optional URL referencing the plugin's documentation. 
        /// </summary>
        string DocUrl { get; }       

        /// <summary>
        /// Invoked by the ITypeResolver implementation to specify runtime information about a plugin.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly containing the plugin.</param>
        /// <param name="assemblyVersion">The version of the assembly containing the plugin.</param>
        /// <param name="pluginTypes">The public types contained within the assembly of the plugin.</param>
        void SetPluginMeta(string assemblyName, string assemblyVersion,
            IEnumerable<Type> pluginTypes);

        /// <summary>
        /// Determines if a given type is defined with a plugin.
        /// </summary>
        /// <param name="pluginType">The type to check if a member of the plugin.</param>
        /// <returns>True if an associated type.  Otherwise, False.</returns>
        bool HasType(Type pluginType);

        /// <summary>
        /// Returns instance of a plugin configuration of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration.</typeparam>
        /// <returns>Reference to the configuration.  If not found, an exception is thrown.</returns>
        T GetConfig<T>() where T : IPluginConfig;
    }
}