using NetFusion.Core.Bootstrap.Plugins;

namespace Solution.Context.WebApi.Plugin;

public class WebApiPlugin : PluginBase
{
    public const string HostId = "[nf:host-id]";
    public const string HostName = "[nf:microservice-name]";

    public override PluginTypes PluginType => PluginTypes.HostPlugin;
    public override string PluginId => HostId;
    public override string Name => HostName;
        
    public WebApiPlugin()
    {
        Description = "WebApi host exposing REST/HAL based Web API.";
    }
}