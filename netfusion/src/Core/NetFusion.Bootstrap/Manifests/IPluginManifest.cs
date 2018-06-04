﻿namespace NetFusion.Bootstrap.Manifests
{
    /// <summary>y 
    /// Base interface used to classify assemblies containing plug-ins that are bootstrapped.
    /// </summary>
    public interface IPluginManifest
    {
        /// <summary>
        /// Uniquely identifies the plug-in.
        /// </summary>
        /// <returns>Unique value.</returns>
        string PluginId { get; }

        /// <summary>
        /// Short name to identify the plug-in's purpose.
        /// </summary>
        /// <returns>Display name.</returns>
        string Name { get; }

        /// <summary>
        /// The name of the assembly containing the manifest.
        /// </summary>
        /// <returns>The name of the assembly.</returns>
        string AssemblyName { get; set; }

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        string AssemblyVersion { get; set; }

        /// <summary>
        /// Description of the functionality implemented by the assembly containing the manifest.
        /// </summary>
        /// <returns>Description of functionality.</returns>
        string Description { get; }

        /// <summary>
        /// URL pointing to source about the plug-in and its use.
        /// </summary>
        string SourceUrl { get; }

        /// <summary>
        /// URL pointing to documentation about the plug-in and its use.
        /// </summary>
        string DocUrl { get; }
    }
}