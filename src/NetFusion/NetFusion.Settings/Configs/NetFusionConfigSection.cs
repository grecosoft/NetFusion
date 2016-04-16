using NetFusion.Bootstrap.Container;
using System.Configuration;

namespace NetFusion.Settings.Configs
{
    /// <summary>
    /// NetFusion settings that can be specified within the host 
    /// application's configuration file.  An instance of this 
    /// class can be added to the AppContainer as a configuration 
    /// since it implements IContainerConfig.
    /// </summary>
    public class NetFusionConfigSection : ConfigurationSection,
        IContainerConfig
    {
        private static ConfigurationProperty HostConfigProp;

        static NetFusionConfigSection()
        {
            HostConfigProp = new ConfigurationProperty("hostConfig", typeof(HostConfigElement), 
                null, ConfigurationPropertyOptions.IsRequired);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection
                {
                    HostConfigProp
                };
            }
        }

        /// <summary>
        /// General application global configurations.
        /// </summary>
        public HostConfigElement HostConfig
        {
            get { return (HostConfigElement)this[HostConfigProp]; }
            set { this[HostConfigProp] = value; }
        }
    }
}
