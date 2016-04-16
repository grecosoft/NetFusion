using System.Configuration;

namespace NetFusion.Settings.Configs
{
    /// <summary>
    /// General host configurations that are read from the host's 
    /// configuration file.
    /// </summary>
    public class HostConfigElement : ConfigurationElement
    {
        private readonly static ConfigurationProperty EnvironmentProp;

        static HostConfigElement()
        {
            EnvironmentProp = new ConfigurationProperty("environment", typeof(EnvironmentTypes), 
                EnvironmentTypes.Development,
                ConfigurationPropertyOptions.IsRequired);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection
                {
                    EnvironmentProp
                };
            }
        }

        /// <summary>
        /// Determines the environment under which the host application should execute.
        /// </summary>
        public EnvironmentTypes Environment
        {
            get { return (EnvironmentTypes)this[EnvironmentProp]; }
            set { this[EnvironmentProp] = value; }
        }
    }
}
