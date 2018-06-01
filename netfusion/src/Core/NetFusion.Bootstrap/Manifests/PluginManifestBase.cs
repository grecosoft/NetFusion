namespace NetFusion.Bootstrap.Manifests
{
    /// <summary>
    /// Base implementation containing properties that are set
    /// automatically by the base infrastructure.
    /// </summary>
    public class PluginManifestBase
    {
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }
        public string MachineName { get; set; }
        public string SourceUrl { get; set; } 
        public string DocUrl { get; set; }
    }
}
