using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Utilities.Validation.Core;
using NetFusion.Utilities.Validation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Settings.Modules
{
    /// <summary>
    /// Provides loading and initializing of application settings when they are dependency injected into a 
    /// dependent component for the first time.  This plug-in module delegates to the Microsoft Extension
    /// Configuration implementation and loads a setting from its associated section.
    /// </summary>
    public class AppSettingsModule : PluginModule
    {
        public override void ScanAllOtherPlugins(TypeRegistration registration)
        {
            // Register all setting classes contained in all of the plug-ins
            // and when dependency injected, populates the settings by calling
            // the configuration implementation. 
            registration.PluginTypes.AssignableTo<IAppSettings>()
                .AsSelf()
                .InstancePerLifetimeScope()
                .OnActivating(LoadConfiguration);
        }

        private void LoadConfiguration(IActivatingEventArgs<object> handler)
        {
            var settings = handler.Instance;
            var sectionName = GetSectionName(settings);

            if (sectionName != null)
            {
                // Lookup settings with the specified section name.
                var configuration = handler.Context.Resolve<IConfiguration>();
                var section = configuration.GetSection(sectionName);

                // Bind the configuration to the settings instance.
                section.Bind(settings);
            }

            // Determine if the settings object can be validated.
            var validator = new ObjectValidator(settings);
            var validationResult = new ValidationResult(settings, validator);

            validationResult.ThrowIfInvalid();
        }

        // Navigates up the settings base types and looks for all ConfigurationSection attributes
        // to build the full path of the settings location.
        private string GetSectionName(object settings)
        {
            var sectionNames = GetSectionNames(settings);
            return String.Join(":", sectionNames.Reverse().ToArray());
        }

        private IEnumerable<string> GetSectionNames(object settings)
        {
            Type baseType = settings.GetType();

            while (baseType != null)
            {
                string sectionName = GetSectionName(baseType);
                if (sectionName != null)
                {
                    yield return sectionName;
                }
                baseType = baseType.GetTypeInfo().BaseType;
            }
        }

        private string GetSectionName(Type settingsType)
        {
            return settingsType.GetAttribute<ConfigurationSectionAttribute>()?.SectionName;
        }

        private void LogAppSettings(IDictionary<string, object> moduleLog)
        {
            moduleLog["Application-Settings"] = Context.AllPluginTypes
                .Where(t =>
                {
                    return t.IsConcreteTypeDerivedFrom<IAppSettings>();
                })
                .Select(t => t.AssemblyQualifiedName);
        }
    }
}
