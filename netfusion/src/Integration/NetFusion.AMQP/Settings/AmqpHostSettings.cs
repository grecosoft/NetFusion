using System.Collections.Generic;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.AMQP.Settings
{
    /// <summary>
    /// AMQP settings used by the application.
    /// </summary>
    [ConfigurationSection("netfusion:amqp")]
    public class AmqpHostSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// The configured hosts.
        /// </summary>
        public ICollection<HostSettings> Hosts { get; set; }

        public AmqpHostSettings()
        {
            Hosts = new List<HostSettings>();
        }
        
        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Hosts);
        }
    }
}