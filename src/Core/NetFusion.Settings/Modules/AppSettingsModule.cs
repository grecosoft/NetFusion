using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NetFusion.Settings.Modules
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
                }
                
                services.Configure(appSettingType, Context.Configuration.GetSection(sectionPath));
            }
        }

        // Register each application setting so a factory method is invoked when injected 
        // into a dependent component.  The factory method delegates to Microsoft's base 
        // implementation.
        public override void ScanAllOtherPlugins(ITypeCatalog catalog)
        {
            catalog.AsDescriptor(
                t => t.IsConcreteTypeDerivedFrom<IAppSettings>(), 
                st => ServiceDescriptor.Singleton(st, sp => {
                    Type optionsType = typeof(IOptions<>).MakeGenericType(st);

                    dynamic options = sp.GetRequiredService(optionsType);
                    object appSettings = options.Value;

                    ValidateSettings(appSettings);
                    return appSettings;
                }));
        }

        private void ValidateSettings(object settings)
        {
            var validator = new DataAnnotationsValidator(settings);
            var result = validator.Validate();

            if (result.IsInvalid)
            {
                string section = SettingsExtensions.GetSectionPath(settings.GetType());

                Context.Logger.LogErrorDetails(
                    $"Invalid application setting: {settings.GetType().FullName}. " + 
                    $"With configuration section: {section}.", result.ObjectValidations);
            }

            result.ThrowIfInvalid();
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
