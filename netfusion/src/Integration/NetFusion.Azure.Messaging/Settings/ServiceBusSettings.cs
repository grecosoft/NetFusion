using System.Collections.Generic;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.Azure.Messaging.Settings
{
    /// <summary>
    /// Settings for the Azure namespaces used by the application.
    /// </summary>
    [ConfigurationSection("netfusion:azure:servicebus")]
    public class ServiceBusSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// The configured namespaces.
        /// </summary>
        public ICollection<NamespaceSettings> Namespaces { get; set; }

        public ServiceBusSettings()
        {
            Namespaces = new List<NamespaceSettings>();
        }
        
        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Namespaces);
        }
    }
}