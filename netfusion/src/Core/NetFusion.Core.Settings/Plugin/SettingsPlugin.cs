using System;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Core.Settings.Plugin.Modules;

namespace NetFusion.Core.Settings.Plugin;

public class SettingsPlugin : PluginBase
{
    public override string PluginId => "1FC4C728-83E0-4407-B846-2871B3F0A1B6";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: Settings";

    public SettingsPlugin()
    {
        AddModule<AppSettingsModule>();
            
        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Core/NetFusion.Core.Settings";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki/core.settings.overview";
            
        Description =  "Plug-in that locates application settings using Microsoft Configuration Extensions and " + 
                       "initializes them when injected into a dependent component.";
    }
}
    
public static class CompositeBuilderExtensions
{
    /// <summary>
    /// Adds the Settings Plugin to the composite container.
    /// </summary>
    /// <param name="composite">Reference to the composite container builder.</param>
    /// <returns>Reference to the composite container builder.</returns>
    public static ICompositeContainerBuilder AddSettings(this ICompositeContainerBuilder composite)
    {
        ArgumentNullException.ThrowIfNull(composite);

        composite.AddPlugin<SettingsPlugin>();
        return composite;
    }
}