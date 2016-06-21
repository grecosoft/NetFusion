using NetFusion.Bootstrap.Container;
using System.Configuration;

namespace NetFusion.Settings.MongoDB.Configs
{
    /// <summary>
    /// Settings that can be specified within the host application's configuration file
    /// used to determine how application settings are loaded from MongoDB.  
    /// An instance of this class can be added to the AppContainer as a configuration 
    /// since it implements IContainerConfig.
    /// </summary>
    public class MongoAppSettingsConfigSection : ConfigurationSection,
        IContainerConfig
    {
        private static ConfigurationProperty MongoAppSettingsConfigProp;

        static MongoAppSettingsConfigSection()
        {
            MongoAppSettingsConfigProp = new ConfigurationProperty("settingsStore", typeof(MongoStoreConfigElement),
                null, ConfigurationPropertyOptions.None);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection
                {
                    MongoAppSettingsConfigProp
                };
            }
        }

        /// <summary>
        /// Settings used to specify where applications settings are stored in MongoDB.
        /// </summary>
        public MongoStoreConfigElement MongoAppSettingsConfig
        {
            get { return (MongoStoreConfigElement)this[MongoAppSettingsConfigProp]; }
            set { this[MongoAppSettingsConfigProp] = value; }
        }
    }
}
