using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Implementations return a list of serializes that should be used
    /// to serialize and deserialize messages.
    /// During the plug-in bootstrap process, the host plug-in and all 
    /// application-component plug-ins are scanned for an implementation. 
    /// </summary>
    public interface IBrokerSerializerRegistry : IKnownPluginType
    {
        /// <summary>
        /// List of event message serializers.
        /// </summary>
        /// <returns>List of serializers.</returns>
        IEnumerable<IBrokerSerializer> GetSerializers();
    }
}
