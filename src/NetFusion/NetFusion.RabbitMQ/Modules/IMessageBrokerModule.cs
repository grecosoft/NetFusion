using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plug-in module service exposed by the Message Broker Module.
    /// This module interface exposes only the most basic functionality
    /// of the module that can be used by other components.
    /// </summary>
    public interface IMessageBrokerModule : IPluginModuleService
    {
       IMessageBroker MessageBroker { get; }
    }
}
