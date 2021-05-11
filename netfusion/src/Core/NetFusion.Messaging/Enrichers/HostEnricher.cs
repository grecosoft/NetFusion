using System.Threading.Tasks;
using NetFusion.Bootstrap.Container;    
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Enricher that tags all messages with the name and identity
    /// of the Microservice from where the message was published.
    /// </summary>
    public class HostEnricher : IMessageEnricher
    {
        private readonly ICompositeApp _compositeApp;
        
        public HostEnricher(ICompositeApp compositeApp)
        {
            _compositeApp = compositeApp;
        }
        
        public Task EnrichAsync(IMessage message)
        {
            message.Attributes.SetStringValue("Microservice", _compositeApp.HostPlugin.Name);
            message.Attributes.SetStringValue("MicroserviceId", _compositeApp.HostPlugin.PluginId);
            return Task.CompletedTask;
        }
    }
}