﻿using System;
using System.Collections.Generic;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Interface for an implementation responsible for resolving types allowing
/// the composite-application to be independent of runtime information such
/// as assemblies.  This greatly simplifies unit-testing.  
/// </summary>
public interface ITypeResolver
{
    /// <summary>
    /// The implementation should invoke the SetPluginMeta method on the passed plugin.
    /// </summary>
    /// <param name="plugin">The plugin to have its metadata set.</param>
    void SetPluginMeta(IPlugin plugin);
        
    /// <summary>
    /// The implementation should check each plugin module for IEnumerable properties where the 
    /// generic parameter derives from IKnownPluginType.  Each of these properties should be set 
    /// to an array containing all derived concrete instances.
    /// </summary>
    /// <param name="plugin">The plugin to be composed.</param>
    /// <param name="fromPluginTypes">The list of types from which the IKnownPluginType derived
    /// instances are created.</param>
    void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes);
}