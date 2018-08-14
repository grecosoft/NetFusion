using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Base.Validation;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Settings;

namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Configuration settings for defined business used by the application.
    /// 
    /// https://github.com/grecosoft/NetFusion/wiki/core.settings.overview#defining-application-settings
    /// https://github.com/grecosoft/NetFusion/wiki/common.validation.overview#class-validation
    /// </summary>
    [ConfigurationSection("netfusion:rabbitMQ")]
    public class BusSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// List of broker connections specified by host application.
        /// </summary>
        public IList<BusConnection> Connections { get; set; }

        public BusSettings()
        {
            Connections = new List<BusConnection>();
        }

        /// <summary>
        /// Returns connection settings for a named bus configuration.
        /// </summary>
        /// <param name="busName">The name of the bus to return the settings.</param>
        /// <returns>The bus related settings.  If a bus with the specified 
        /// name is not defined, an exception is raised.</returns>
        public BusConnection GetConnection(string busName)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));

            BusConnection conn = Connections.FirstOrDefault(c => c.BusName == busName);
            
            if (conn == null)
            {
                throw new InvalidOperationException($"Bus connection with name: {busName} not configured.");
            }
            return conn;
        }

        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Connections);
        }
    }
}