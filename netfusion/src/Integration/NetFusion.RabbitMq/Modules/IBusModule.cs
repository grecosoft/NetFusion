using EasyNetQ;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plugin module service used to obtain IBus named instances.
    /// </summary>
    public interface IBusModule : IPluginModuleService
    {
        /// <summary>
        /// Returns a configured IBus instance based on its configured name.
        /// If no bus exists for the provided name, an exception is raised.
        /// </summary>
        /// <param name="named">The name of the configured bus to locate.</param>
        /// <returns>Reference to the IBus instance.</returns>
        IBus GetBus(string named);

        /// <summary>
        /// Returns exchange configuration settings stored external to the code.
        /// </summary>
        /// <param name="busName">The name of the bus.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <returns>The exchange settings if found.  Otherwise, null is
        /// returned.  If a bus configuration cannot be found, an exception
        /// will be raised.</returns>
        ExchangeSettings GetExchangeSettings(string busName, string exchangeName);
        
        /// <summary>
        /// Returns queue configuration settings stored external to the code.
        /// </summary>
        /// <param name="busName">The name of the bus.</param>
        /// <param name="queueName">The name of the queue.</param>
        /// <returns>The queue settings if found.  Otherwise, null is 
        /// returned.  If a bus configuration cannot be found, an exception
        /// will be raised.</returns>
        QueueSettings GetQueueSettings(string busName, string queueName);
    }
}