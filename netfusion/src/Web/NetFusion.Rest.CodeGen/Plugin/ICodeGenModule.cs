using NetFusion.Rest.CodeGen.Plugin.Configs;

namespace NetFusion.Rest.CodeGen.Plugin
{
    public interface ICodeGenModule
    {
        RestCodeGenConfig CodeGenConfig { get; }
    }
}
