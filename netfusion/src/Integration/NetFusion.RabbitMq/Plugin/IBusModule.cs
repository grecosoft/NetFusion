using EasyNetQ;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Plugin
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
        /// The identity value of the host application plugin.  This can be
        /// appended to queue names to make them application specific.
        /// </summary>
        string HostAppId { get; }

        /// <summary>
        /// Applies any exchange settings stored within application settings.
        /// These settings override any specified within code.
        /// </summary>
        /// <param name="meta">Metadata about an exchange to be created.</param>
        void ApplyExchangeSettings(ExchangeMeta meta);
        
        /// <summary>
        /// Applies any queue settings stored within application settings.
        /// These settings override any specified within code.
        /// </summary>
        /// <param name="meta">Metadata about a queue to be created.</param>
        void ApplyQueueSettings(QueueMeta meta);
    }
}