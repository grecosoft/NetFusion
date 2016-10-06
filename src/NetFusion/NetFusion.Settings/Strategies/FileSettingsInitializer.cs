using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Serialization;
using NetFusion.Settings.Modules;
using System;
using System.Collections.Generic;
using System.IO;

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
        private const string CONFIG_ROOT_DIR = "Configs";
        private const string CONFIG_FILE_EXT = "json";

        private IDictionary<EnvironmentTypes, string> _envDirMappings;
    
        public FileSettingsInitializer(IAppSettingsModule appSettingsModule)
        {
            _envDirMappings = new Dictionary<EnvironmentTypes, string> {
                { EnvironmentTypes.Development, "Dev" },
                { EnvironmentTypes.Test, "Test" },
                { EnvironmentTypes.Production, "Prod" }
            };
        }

        protected override IAppSettings OnConfigure(TSettings settings)
        {
            string envName = _envDirMappings[settings.Environment];

            string machineEnvPath = GetSettingsConfigFilePath(settings.MachineName, envName);
            string machinePath = GetSettingsConfigFilePath(settings.MachineName);
            string envPath = GetSettingsConfigFilePath(environmentName: envName);
            string configPath = GetSettingsConfigFilePath();
        
            return LoadSettingsFromPath(machineEnvPath) 
                ?? LoadSettingsFromPath(machinePath) 
                ?? LoadSettingsFromPath(envPath) 
                ?? LoadSettingsFromPath(configPath);
        }

        private string AssemblyProbeDirectory
        {
            get { return AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory; }
        }

        private string GetSettingsConfigFilePath(string machineName = "", string environmentName = "")
        {
            string appBaseDir = Path.Combine(this.AssemblyProbeDirectory, CONFIG_ROOT_DIR);
            var fileName = Path.ChangeExtension(this.SettingsType.Name, CONFIG_FILE_EXT);

            return Path.Combine(appBaseDir, machineName, environmentName, fileName);
        }

        private IAppSettings LoadSettingsFromPath(string settingsPath)
        {
            IContainerLogger logger = AppContainer.Instance.Logger.ForContext(this.GetType());
            if (File.Exists(settingsPath))
            {
                logger.Debug($"Settings of type: {typeof(TSettings)} loaded from the following location: {settingsPath}.");

                return JsonUtility.Deserialize<TSettings>(File.OpenText(settingsPath));
            }

            logger.Debug($"Settings of type: {typeof(TSettings)} searched but not found at following location: {settingsPath}.");
            return null;
        }
    }
}
