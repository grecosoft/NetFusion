using NetFusion.Bootstrap.Refactors;

namespace NetFusion.Settings.Plugin
{
    public class SettingsPlugin : PluginDefinition
    {
        public override string PluginId => "1FC4C728-83E0-4407-B846-2871B3F0A1B6";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.CorePlugin;
        public override string Name => "Settings Plug-in";

        public SettingsPlugin()
        {
            AddModule<AppSettingsModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Core/NetFusion.Settings";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/core.settings.overview";
            
            Description =  "Plug-in that locates application settings using Microsoft Configuration Extensions and " + 
                           " initializes them when injected into a dependent component for the first time.";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddSettings(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<SettingsPlugin>();
            return composite;
        }
    }
}