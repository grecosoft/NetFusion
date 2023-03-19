using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.CodeGen.Plugin.Configs;

namespace NetFusion.Web.Rest.CodeGen.Plugin;

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