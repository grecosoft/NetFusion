using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------
        
        public override void RegisterServices(IServiceCollection services)
        {
            IEnumerable<Type> appSettingTypes = Context.AllPluginTypes
                .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>());

            foreach (Type appSettingType in appSettingTypes)
            {
                string sectionPath = SettingsExtensions.GetSectionPath(appSettingType);

                if (string.IsNullOrWhiteSpace(sectionPath))
                {
                    Context.BootstrapLogger.Add(LogLevel.Warning,
                        $"The section path for setting type: {appSettingType.AssemblyQualifiedName} could " + 
                        $"not be determined. Make sure the attribute: {typeof(ConfigurationSectionAttribute)} is specified.");
                    
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
        public override void ScanPlugins(ITypeCatalog catalog)
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
        
        //------------------------------------------------------
        //--Plugin Execution
        //------------------------------------------------------

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
