using NetFusion.Bootstrap.Plugins;

namespace NetFusion.RabbitMQ.Plugin.Configs
{
    /// <summary>
    /// Overall RabbitMq plugin configurations. 
    /// </summary>
    public class RabbitMqConfig : IPluginConfig
    {
        /// <summary>
        /// Determines if RabbitMQ logs should be forwarded to Microsoft's base ILogger.  If not specified,
        /// the underlying EasyNetQ library will automatically forward logs if one of the supported loggers
        /// are configured by the host application (such as Serilog).
        ///
        /// https://github.com/EasyNetQ/EasyNetQ/wiki/Logging
        /// </summary>
        public bool DelegateToBaseLogger { get; set; }
    }
}