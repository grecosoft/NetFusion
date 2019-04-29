using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Settings.Plugin
{
    /// <summary>
    /// Extends the Microsoft configuration options by allowing setting types to be defined 
    /// and automatically registered with the service collection.  Dependent components can 
    /// directly inject instances of the application settings.  When a given settings class
    /// is injected, it delegates to Microsoft's base implementation and loads the settings 
    /// using the configuration path determined by the ConfigurationSection attributes.  
    /// The settings are also validated before being returned.
    /// </summary>
    public class AppSettingsModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            IEnumerable<Type> appSettingTypes = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>());

            foreach (Type appSettingType in appSettingTypes)
            {
                string sectionPath = SettingsExtensions.GetSectionPath(appSettingType);

                if (string.IsNullOrWhiteSpace(sectionPath))
                {
                    Context.Logger.LogWarning(
                        $"The section path for setting type: {appSettingType.AssemblyQualifiedName} could " + 
                        $"not be determined. Make sure the attribute: {typeof(ConfigurationSectionAttribute)} is specified.");
                    
                    continue;
                }
                
                services.Configure(appSettingType, Context.Configuration.GetSection(sectionPath));
            }
        }

        // Register each application setting so a factory method is invoked when injected 
        // into a dependent component.  The factory method delegates to Microsoft's base 
        // implementation.
        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsDescriptor(
                t => t.IsConcreteTypeDerivedFrom<IAppSettings>(), 
                st => ServiceDescriptor.Singleton(st, sp => {
                    Type optionsType = typeof(IOptions<>).MakeGenericType(st);

                    dynamic options = sp.GetRequiredService(optionsType);
                    object appSettings = options.Value;

                    SettingsExtensions.ValidateSettings(Context.Logger, appSettings);
                    return appSettings;
                }));
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Application:Settings"] = Context.AllPluginTypes
               .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>())
               .Select(t => new {
                   SettingsClass = t.AssemblyQualifiedName,
                   SectionPath = SettingsExtensions.GetSectionPath(t)
               });
        }
    }
}
