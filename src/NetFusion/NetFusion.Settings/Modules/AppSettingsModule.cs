using Autofac;
using Autofac.Core;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Common.Validation;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Settings.Modules
{
    /// <summary>
    /// Provides loading and initializing of application settings when they are dependency injected into
    /// a dependent component for the first time.  How the settings are loaded is the responsibility of 
    /// IAppSettingInitializer implementations.  There can be only one settings initializer for a given 
    /// settings class specified within an application plug-in.  If more than one is found, an exception
    /// is thrown.
    /// 
    /// Open-generic setting initializers can be registered by the host application to be invoked when
    /// a class specific settings initializer is not found or cannot populate the setting.
    /// </summary>
    public class AppSettingsModule : PluginModule,
        IAppSettingsModule
    {
        // IAppSettingsModule:
        public NetFusionConfig AppConfig { get; private set; }

        public override void Initialize()
        {
            bool isConfigSet = this.Context.Plugin.IsConfigSet<NetFusionConfig>();
            this.AppConfig = this.Context.Plugin.GetConfig<NetFusionConfig>();

            // Only apply the settings from the configuration file if the host
            // application didn't manually specify settings during bootstrap.
            if (!isConfigSet)
            {
                SetHostAppConfigFileSettings(this.AppConfig);
            }

            AssertNoDuplicateSettingInitializers();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // If the host application specified default open-generic 
            // settings initializers, add them to the container.  
            this.AppConfig.SettingInitializerTypes.ForEach(si =>
            {
                builder.RegisterGeneric(si)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            });
        }

        public override void ScanAllOtherPlugins(TypeRegistration registration)
        {
            // Register all setting classes contained in all of the plug-ins
            // and when dependency injected for the first time, populates the
            // setting.
            registration.PluginTypes.AssignableTo<IAppSettings>()
                .AsSelf()
                .SingleInstance()
                .OnActivating(InitializeSettings);
        }

        public override void ScanApplicationPluginTypes(TypeRegistration registration)
        {
            // Register all setting initializer classes defined for a specific
            // application settings class.
            registration.PluginTypes
                .AsClosedTypesOf(typeof(IAppSettingsInitializer<>))
                .InstancePerLifetimeScope();
        }

        // Set the values based on a container configuration that are read from the
        // application host's configuration file.  If the host didn't specify the
        // NetFusionSection configuration, use default settings.  
        private void SetHostAppConfigFileSettings(NetFusionConfig appConfig)
        {
            if (!this.Context.Plugin.IsConfigSet<NetFusionConfigSection>()) return;

            var configSection = this.Context.Plugin.GetConfig<NetFusionConfigSection>();
            var hostConfig = configSection.HostConfig;

            if (hostConfig != null)
            {
                appConfig.Environment = hostConfig.Environment;
            }
        }
        
        // There can be at most one closed-generic setting initializer per application
        // setting. 
        private void AssertNoDuplicateSettingInitializers()
        {
            var duplicates = new List<Type>();

            // List of all possible closed setting initializers.
            var settingInitTypes = this.Context.GetPluginTypesFrom()
                .Where(t => t.IsDerivedFrom<IAppSettings>() && t.IsClass)
                .Select(st => typeof(IAppSettingsInitializer<>).MakeGenericType(st))
                .ToList();

            // All initializers.
            var allInitializers = this.Context.GetPluginTypesFrom()
                .Where(t => t.IsDerivedFrom<IAppSettingsInitializer>() && t.IsClass && !t.IsAbstract)
                .ToList();

            // If more than one setting initializer is derived from one of the setting
            // specific closed generic types, then there is duplicate.
            foreach (Type settingInitType in settingInitTypes)
            {
                if (allInitializers.Count(it => it.IsDerivedFrom(settingInitType)) > 1)
                {
                    duplicates.Add(settingInitType);
                }
            }
         
            if (duplicates.Any())
            {
                throw new ContainerException(
                    $"There were duplicate setting initializers for the following setting types: " +
                    $"{String.Join(", ", duplicates.Select(st => st.FullName))}.");
            }
        }

        // Called when the settings class is dependency injected into a component for the first time.
        // The following determines which ISettingsInitializer should be used to load the application
        // settings class.
        private void InitializeSettings(IActivatingEventArgs<object> settingLoadEvent)
        {
            var settings = (IAppSettings)settingLoadEvent.Instance;
            
            // Set properties that can be used by the initializer to load the settings.
            settings.ApplicationId = this.Context.AppHost.Manifest.PluginId;
            settings.Environment = this.AppConfig.Environment;

            IAppSettings initializedSettings = null;

            // First see if there is a settings initializer specific to the settings type.
            var settingSpecificInitializer = GetSettingSpecificInitializer(settings, settingLoadEvent.Context);
            initializedSettings = settingSpecificInitializer?.Configure(settings);
           
            if (initializedSettings == null)
            {
                // If a settings specific initializer has not been found, or couldn't populate the settings,
                // apply in order the non-type specific initializers that have been configured by the host 
                //application.  Stop after finding the initializer that is able to initialize the settings.
                foreach (Type settingInitType in this.AppConfig.SettingInitializerTypes)
                {
                    var settingInitializer = GetSettingGenericInitializer(settings, settingInitType, settingLoadEvent.Context);

                    initializedSettings = settingInitializer.Configure(settings);
                    if (initializedSettings != null) break;
                }
            }

            if (initializedSettings == null)
            {
                if (settings.IsInitializationRequired)
                {
                    throw new ContainerException(
                    $"The setting type: {settings} could not be initialized by any of the available setting initializers.");
                }

                initializedSettings = settings;
            }
            else if (initializedSettings != settings)
            {
                // If the initializer returned a new instance, update it in the container
                // to be used as the new singleton instance.
                settingLoadEvent.ReplaceInstance(initializedSettings);
            }

            // The settings class may be decorated with .NET validation attributes.
            initializedSettings.Validate().ThrowIfNotValid();
        }

        private IAppSettingsInitializer GetSettingSpecificInitializer(
            IAppSettings settings,
            IComponentContext context)
        {
            var settingSpecificType = typeof(IAppSettingsInitializer<>).MakeGenericType(settings.GetType());

            var specificSettingInit = this.Context.GetPluginTypesFrom(PluginTypes.AppHostPlugin, PluginTypes.AppComponentPlugin)
                .FirstOrDefault(t => t.IsDerivedFrom(settingSpecificType));

            if (specificSettingInit != null)
            {
                return context.Resolve(specificSettingInit) as IAppSettingsInitializer;
            }

            return null;
        }

        // Returns a setting initializer that is based on an open-generic type that can load settings
        // for any type of application setting.
        private IAppSettingsInitializer GetSettingGenericInitializer(
            IAppSettings settings, 
            Type genericIntitializerType,
            IComponentContext context)
        {
            var closedSettingType = genericIntitializerType.MakeGenericType(settings.GetType());
            return (IAppSettingsInitializer)context.Resolve(closedSettingType);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            LogSettingInits(moduleLog);
            LogAppSettings(moduleLog);
        }

        private void LogSettingInits(IDictionary<string, object> moduleLog)
        {
            // All the setting initializers with the configured ones added in configuration order.
            var settingInits = Context.GetPluginTypesFrom()
                .Where(t => t.IsDerivedFrom<IAppSettingsInitializer>() && t.IsClass && !t.IsAbstract)

                // Exclude open generic initializers and add them back to the end.
                .Except(this.AppConfig.SettingInitializerTypes)
                .Concat(this.AppConfig.SettingInitializerTypes);
            
            moduleLog["Setting-Initializers"] = settingInits
                .Select(t => new {
                    InitializerType = t.AssemblyQualifiedName,
                    IsConfigured = this.AppConfig.SettingInitializerTypes.Contains(t)
                });
        }

        private void LogAppSettings(IDictionary<string, object> moduleLog)
        {
            moduleLog["Application-Settings"] = Context.GetPluginTypesFrom()
                .Where(t => t.IsDerivedFrom<IAppSettings>() && t.IsClass && !t.IsAbstract)
                .Select(t => t.AssemblyQualifiedName);
        }
    }
}
