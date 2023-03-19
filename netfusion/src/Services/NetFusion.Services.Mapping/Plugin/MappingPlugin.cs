using System;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Services.Mapping.Plugin.Modules;

namespace NetFusion.Services.Mapping.Plugin;

public class MappingPlugin : PluginBase
{
    public override string PluginId => "83C90E78-D245-4B0D-A4FC-E74B11227766";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: Mapping";

    public MappingPlugin()
    {
        AddModule<MappingModule>();
            
        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Common/NetFusion.Services.Mapping";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.mapping.overview";
            
        Description =  "Plug-In used to map object instances to other object representations.  This plug-in " +
                       "configures and coordinates the mapping process but does not dependent on any specific " +
                       "mapping library.  The host application can use an open-source mapping library of choice.";
    }
}
    
public static class CompositeBuilderExtensions
{
    public static ICompositeContainerBuilder AddMapping(this ICompositeContainerBuilder composite)
    {
        if (composite == null) throw new ArgumentNullException(nameof(composite));
            
        // Add mapping plugin:
        return composite.AddPlugin<MappingPlugin>();
    }
}