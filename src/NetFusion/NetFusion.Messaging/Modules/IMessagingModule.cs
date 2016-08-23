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
        ILookup<Type, MessageDispatchInfo> InProcessDispatchers { get; }

        /// <summary>
        /// Returns the in-process dispatcher associated with the specified command type.
        /// </summary>
        /// <param name="commandType">The type of the command to find associated dispatcher.</param>
        /// <returns>Command message dispatcher metadata information.</returns>
        /// <exception cref="InvalidOperationException">Exception if one and only one dispatcher
        /// can't be found the specified command.</exception>
        MessageDispatchInfo GetInProcessCommandDispatcher(Type commandType);

        /// <summary>
        /// Invokes the consumer with the message defined by the dispatcher instance.
        /// </summary>
        /// <param name="dispatcher">The dispatcher containing information on how the message
        /// is to be dispatched.</param>
        /// <param name="message">The message to dispatch to the consumer.</param>
        /// <returns>The result from the consumer.  If the message is a command and the response
        /// is assignable to it response type, it is automatically set on the command.</returns>
        /// <exception cref="InvalidOperationException">Exception if the type of message is not
        /// is not the same type associated with the dispatcher.
        /// </exception>
        Task<object> InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message);


        Task<T> InvokeDispatcher<T>(MessageDispatchInfo dispatcher, IMessage message)
           where T : class;

        Task<T> DispatchConsumer<T>(IMessage message, MessageDispatchInfo dispatchInfo)
            where T : class;
    }
}
