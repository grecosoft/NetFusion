using NetFusion.Bootstrap.Refactors;

namespace Service.WebApi.Plugin
{
    public class WebApiPlugin : PluginDefinition
    {
        public override string PluginId => "fddc1d2d-2f86-4d96-a1a8-e3de72c1a02a";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.HostPlugin;
        public override string Name => "WebApi host exposing REST/HAL based Web API.";
    }
}