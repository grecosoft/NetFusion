using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;

namespace NetFusion.Settings.Configs
{
    /// <summary>
    /// Used by the application host to specify global application 
    /// configurations.
    /// </summary>
    public class NetFusionConfig : IContainerConfig
    {
        private List<Type> _initializers = new List<Type>();

        /// <summary>
        /// Initializer strategy types that are invoked to load application settings.
        /// </summary>
        public IEnumerable<Type> SettingInitializerTypes { get { return _initializers; } }

        /// <summary>
        /// Indicates the environment in which the application is executing.  This value
        /// can be specified within the application's configuration file by configuring
        /// the NetFusionConfigSection.  If this configuration section is specified,
        /// the configured value will override any value specified in code.
        /// </summary>
        public EnvironmentTypes Environment { get; set; } = EnvironmentTypes.Development;

        /// <summary>
        /// Adds an open-generic application settings initializer to the
        /// container.  The order in which the setting initializer types
        /// are specified is the order in which they are called to populate
        /// a setting's values.
        /// </summary>
        /// <param name="settingInitTypes">One or more open-generic setting
        /// initializers strategies.</param>
        public void AddSettingsInitializer(params Type[] settingInitTypes)
        {
            foreach (var settingInitType in settingInitTypes)
            {
                Check.IsTrue(
                    settingInitType.IsOpenGenericType(),
                    nameof(settingInitTypes),
                    $"specified settings initializer must be an open-generic type");
            }
            
            _initializers.AddRange(settingInitTypes);
        }
    }
}
