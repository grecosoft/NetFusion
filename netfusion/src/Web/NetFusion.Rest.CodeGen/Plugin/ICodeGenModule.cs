using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.CodeGen.Plugin.Configs;

namespace NetFusion.Rest.CodeGen.Plugin
{
    /// <summary>
    /// Plugin module service interface exposing information about the code-generation.
    /// </summary>
    public interface ICodeGenModule : IPluginModuleService
    {
        /// <summary>
        /// Reference to the plugin configuration containing code-generation settings.
        /// </summary>
        RestCodeGenConfig CodeGenConfig { get; }
    }
}
