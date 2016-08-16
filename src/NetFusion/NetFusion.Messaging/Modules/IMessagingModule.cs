using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// Services exposed by the messaging module for access by other 
    /// application modules and services.
    /// </summary>
    public interface IMessagingModule : IPluginModuleService
    {
        /// <summary>
        /// The associated messaging configuration.
        /// </summary>
        MessagingConfig MessagingConfig { get; }

        /// <summary>
        /// A lookup by message type used to find the dispatch information for all
        /// consumer message handlers that should  be invoked for a given type of message.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> AllMessageTypeDispatchers { get; }

        /// <summary>
        /// A lookup by message types used to find the dispatch information for all
        /// consumer message handlers that should  be invoked for a given type of message
        /// marked with the InProcessHandler attribute.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> InProcessMessageTypeDispatchers { get; }

        /// <summary>
        /// Dispatches a message using the specified dispatch information.
        /// </summary>
        /// <param name="message">The message to dispatch.</param>
        /// <param name="dispatchInfo">Contains information about the consumer that should
        /// receives the message.</param>
        /// <returns>The future result of the dispatch returned by the consumer.</returns>
        Task<T> DispatchConsumer<T>(IMessage message, MessageDispatchInfo dispatchInfo)
            where T : class;
    }
}
