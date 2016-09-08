using NetFusion.Common.Serialization;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetFusion.Settings.Strategies
{
    /// <summary>
    /// Initializes settings from local JSON configuration files.  This can be used
    /// in a production environment or during development to override global application
    /// settings loaded by subsequent configuration initializer strategies.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings to load.</typeparam>
    public class FileSettingsInitializer<TSettings> : AppSettingsInitializer<TSettings>
        where TSettings : IAppSettings
    {
        private IDictionary<EnvironmentTypes, string> _envDirMappings;
        private IAppSettingsModule _appSettingsModule;
    
        public FileSettingsInitializer(IAppSettingsModule appSettingsModule)
        {
            _envDirMappings = new Dictionary<EnvironmentTypes, string> {
                { EnvironmentTypes.Development, "Dev" },
                { EnvironmentTypes.Test, "Test" },
                { EnvironmentTypes.Production, "Prod" }
            };

            _appSettingsModule = appSettingsModule;
        }

        protected override IAppSettings OnConfigure(TSettings settings)
        {
            NetFusionConfig appConfig = _appSettingsModule.AppConfig;

            // Check if a settings file specific for the machine and environment exists:
            string machineName = Environment.MachineName.ToLower();
            string settingsPath = GetSettingsConfigFilePath(appConfig, machineName);
            if (File.Exists(settingsPath))
            {
                return JsonUtility.Deserialize<TSettings>(File.OpenText(settingsPath));
            }

            // Check if a settings file for the environment exists:
            settingsPath = GetSettingsConfigFilePath(appConfig); 
            if (File.Exists(settingsPath))
            {
                return JsonUtility.Deserialize<TSettings>(File.OpenText(settingsPath));
            }

            return null;
        }

        private string GetSettingsConfigFilePath(NetFusionConfig appConfig, string machineName = "")
        {
            string appBaseDir = Path.Combine(this.AssemblyProbeDirectory, "Configs");
            string envName = _envDirMappings[appConfig.Environment];
            var fileName = Path.ChangeExtension(this.SettingsType.Name, "json");

            return Path.Combine(appBaseDir, machineName, envName, fileName);
        }

        private string AssemblyProbeDirectory
        {
            get { return AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory; }
        }

    }
}
