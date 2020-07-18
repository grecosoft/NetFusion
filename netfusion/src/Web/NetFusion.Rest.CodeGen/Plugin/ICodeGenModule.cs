using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.CodeGen.Plugin.Configs;

namespace NetFusion.Rest.CodeGen.Plugin
{
    public interface ICodeGenModule : IPluginModuleService
    {
        RestCodeGenConfig CodeGenConfig { get; }
    }
}
