using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetFusion.Base;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Settings.Plugin.Modules
{
    /// <summary>
    /// Extends the Microsoft configuration options by allowing setting types to be defined 
    /// and automatically registered with the service collection.  Dependent components can 
    /// directly inject instances of the application settings.  When a given settings class
    /// is injected, it delegates to Microsoft's base implementation and loads the settings 
    /// using the configuration path determined by the ConfigurationSection attribute.  
    /// The settings are also validated before being returned.
    /// </summary>
    public class AppSettingsModule : PluginModule
    {
        // ------------------------ [Plugin Initialization] --------------------------
        
        public override void RegisterServices(IServiceCollection services)
        {
            IEnumerable<Type> appSettingTypes = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>());

            foreach (Type appSettingType in appSettingTypes)
            {
                string sectionPath = SettingsExtensions.GetSectionPath(appSettingType);

                if (string.IsNullOrWhiteSpace(sectionPath))
                {
                    NfExtensions.Logger.Log<AppSettingsModule>(LogLevel.Warning, 
                        "The section path for settings {SettingsType} could not be determined.  Make sure the " +
                        "attribute {AttributeType} is specified.",
                        appSettingType.AssemblyQualifiedName, 
                        typeof(ConfigurationSectionAttribute));

                    continue;
                }
                
                // This is a non-generic version that is automatically called and adds the
                // configuration-setting to the container. Eliminates having to manually make
                // call for each setting.  This is what enables IOptions and IOptionsSnapshot etc.
                services.Configure(appSettingType, Context.Configuration.GetSection(sectionPath));
            }
        }

        // Register each application setting so a factory method is invoked when injected 
        // into a dependent component.  The factory method delegates to Microsoft's base 
        // implementation.
        public override void ScanForServices(ITypeCatalog catalog)
        {
            catalog.AsDescriptor(
                t => t.IsConcreteTypeDerivedFrom<IAppSettings>(), 
                st => ServiceDescriptor.Singleton(st, sp =>
                {
                    // Note IConfiguration is not being used so the setting that is directly
                    // injected will equal the value if IOption<T> is injected.
                    var optionType = typeof(IOptions<>).MakeGenericType(st);
                    
                    dynamic option = sp.GetRequiredService(optionType);
                    IAppSettings settings = option.Value;
                    
                    SettingsExtensions.ValidateSettings(settings);

                    return settings;
                }));
        }
        
        // ------------------------- [Logging] ------------------------------

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["ApplicationSettings"] = Context.AllPluginTypes
               .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>())
               .Select(t => new {
                   SettingsClass = t.AssemblyQualifiedName,
                   SectionPath = SettingsExtensions.GetSectionPath(t)
               });
        }
    }
}
