using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using System;
using System.Linq;

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
        /// A lookup by message types used to find the dispatch information for all
        /// consumer message handlers that should  be invoked for a given type of message.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> AllMessageTypeDispatchers { get; }

        /// <summary>
        /// A lookup by message types used to find the dispatch information for all
        /// consumer message handlers that should  be invoked for a given type of message
        /// marked with the InProcessHandler attribute.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> InProcessMessageTypeDispatchers { get; }
    }
}
