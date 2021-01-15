using System.Collections.Generic;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.Azure.ServiceBus.Settings
{
    /// <summary>
    /// Settings specifying Azure Service Bus namespace configurations.
    /// </summary>
    [ConfigurationSection("netfusion:azure:serviceBus")]
    public class BusSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// The configured namespaces.
        /// </summary>
        public ICollection<NamespaceSettings> Namespaces { get; set; } = new List<NamespaceSettings>();

        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Namespaces);
        }
    }
}