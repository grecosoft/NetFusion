using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
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
            var sectionName = GetSettingsTypeSectionName(settings.GetType());

            if (sectionName != null)
            {
                // Lookup settings with the specified section name.  IConfiguration
                // is a service provided by MS Configuration Extensions.
                var configuration = handler.Context.Resolve<IConfiguration>();
                var section = configuration.GetSection(sectionName);

                // Bind the configuration to the settings instance.
                section.Bind(settings);
            }

            // Determine if the settings object can be validated.  Note:  The validation implementation
            // is being directly created and not using the host specified implementation so all settings
            // can be consistently validated.  But all other application validation delegates to the 
            // host specified implementation.
            var validator = new MSObjectValidator(settings);
            var result = validator.Validate();

            if (result.IsInvalid)
            {
                Context.Logger.LogErrorDetails(
                    $"Invalid application setting: {settings.GetType().FullName}. With section name: {sectionName}.", 
                    result.ObjectValidations);
            }
            
            result.ThrowIfInvalid();
        }

        // Navigates up the settings base types and looks for all ConfigurationSection attributes
        // to build the full path of the settings location.
        private string GetSettingsTypeSectionName(Type settingsType)
        {
            var sectionNames = GetSectionNames(settingsType);
            return string.Join(":", sectionNames.Reverse().ToArray());
        }

        private IEnumerable<string> GetSectionNames(Type settingsType)
        {
            while (settingsType != null)
            {
                string sectionName = GetSectionName(settingsType);
                if (sectionName != null)
                {
                    yield return sectionName;
                }
                settingsType = settingsType.GetTypeInfo().BaseType;
            }
        }

        private string GetSectionName(Type settingsType)
        {
            return settingsType.GetAttribute<ConfigurationSectionAttribute>()?.SectionName;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Application:Settings"] = Context.AllPluginTypes
               .Where(t => t.IsConcreteTypeDerivedFrom<IAppSettings>())
               .Select(t => new {
                   SettingsClass = t.AssemblyQualifiedName,
                   ConfigSectionName = GetSettingsTypeSectionName(t)
               });
        }
    }
}
