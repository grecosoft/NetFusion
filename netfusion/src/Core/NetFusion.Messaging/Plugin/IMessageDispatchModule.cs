using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.Plugin
{
    /// <summary>
    /// Services exposed by the messaging module for access by other plugin modules.
    /// </summary>
    public interface IMessageDispatchModule : IPluginModuleService
    {
        /// <summary>
        /// The associated messaging configuration.
        /// </summary>
        MessageDispatchConfig DispatchConfig { get; }

        /// <summary>
        /// A lookup by message type used to find the dispatch information for all
        /// consumer message handlers that should be invoked for a given message.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> AllMessageTypeDispatchers { get; }

        /// <summary>
        /// A lookup by message type used to find the dispatch information for all
        /// consumer message handlers that should  be invoked for a given message
        /// marked with the InProcessHandler attribute.
        /// </summary>
        ILookup<Type, MessageDispatchInfo> InProcessDispatchers { get; }

        /// <summary>
        /// Returns the in-process dispatcher associated with the specified command type.
        /// </summary>
        /// <param name="commandType">The type of the command to find associated dispatcher.</param>
        /// <returns>Command message dispatcher metadata information.</returns>
        MessageDispatchInfo GetInProcessCommandDispatcher(Type commandType);

        /// <summary>
        /// Invokes the consumer with the message handler defined by the dispatcher instance.
        /// </summary>
        /// <param name="dispatcher">The dispatcher containing information on how the message
        /// is to be dispatched.</param>
        /// <param name="message">The command to dispatch to the consumer.</param>
        /// <param name="cancellationToken">Optional task cancellation token.</param>
        /// <returns>The result from the consumer.  If the message is a command and the response
        /// is assignable to its response type, it is automatically set on the command.</returns>
        Task<object> InvokeDispatcherInNewLifetimeScopeAsync(MessageDispatchInfo dispatcher, IMessage message, 
            CancellationToken cancellationToken = default(CancellationToken));

    }
}
