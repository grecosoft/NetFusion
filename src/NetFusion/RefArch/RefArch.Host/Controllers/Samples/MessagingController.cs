using NetFusion.Messaging;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Messaging.Messages;
using RefArch.Api.Messaging.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    /// <summary>
    /// These examples show using the Messaging Plug-in for publishing local domain
    /// events and commands.  By default, the InProcessEventDispatcher is configured which
    /// implements the dispatching of messages to handlers defined within the local process.
    /// The messaging plug-in defines an extension point where other publisher implementations
    /// can be called to handle the dispatching of messages (i.e. RabbitMQ).  
    /// 
    /// A message can be published to one or more event handlers.  Message handlers can be
    /// synchronous or asynchronous.  The message handler definition determines if the handler
    /// is invoked synchronously or asynchronously and not the domain-event or command.
    /// The domain-event or command is just a simple POCO.  Since message handlers can be 
    /// either synchronous or asynchronous,  the method to publish messages is asynchronous.
    /// </summary>
    [EndpointMetadata(EndpointName = "NetFusion.Messaging", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/messaging")]
    public class MessagingController : ApiController
    {
        private IMessagingService _messagingService;

        public MessagingController(
            IMessagingService messagingSrv)
        {
            _messagingService = messagingSrv;
        }

        /// <summary>
        /// This example shows publishing a domain event to two synchronous handlers. 
        /// Each handler method puts the current thread to sleep for the specified
        /// number of seconds.  There are two handlers for this event so the time
        /// to execute will be 2 x specified-seconds.
        /// </summary>
        [HttpPost, Route("sync-event", Name = "PublishDomainEventSync")]
        public async Task<MessageResponse> PublishDomainEventSync(MessageInfo info)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var evt = new ExampleSyncDomainEvent(info);
            await _messagingService.PublishAsync(evt);

            stopwatch.Stop();
            return new MessageResponse { Elapsed = stopwatch.Elapsed };
        }

        /// <summary>
        /// This example shows publishing a domain event to two asynchronous handlers.  Each handler 
        /// creates a new task that puts the current thread to sleep for the specified number of seconds.  
        /// There are two handlers for this event that are asynchronously executed so the time should be
        /// about 1 x specified-seconds.
        /// </summary>
        [HttpPost, Route("async-event", Name = "PublishDomainEventAsync")]
        public async Task<MessageResponse> PublishDomainEventAsync(MessageInfo info)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var evt = new ExampleAsyncDomainEvent(info);
            await _messagingService.PublishAsync(evt);

            stopwatch.Stop();
            return new MessageResponse { Elapsed = stopwatch.Elapsed };
        }

        /// <summary>
        /// This example publishes an event that is handled by two asynchronous handlers
        /// that each throw an exception.  The details of the PublisherException are
        /// returned to the calling client.
        /// </summary>
        [HttpPost Route("async-exception", Name = "PostAsyncEventException")]
        public async Task PostAsyncEventException(MessageExInfo info)
        {
            var evt = new ExampleAsyncDomainEventException(info);
            await _messagingService.PublishAsync(evt);
        }

        /// <summary>
        /// This example publishes a command with a typed result.  A command 
        /// message type can have one and only one handler.  If more than one
        /// handler is found, an exception will be thrown during the bootstrap
        /// process.
        /// </summary>
        [HttpPost Route("async-command", Name = "PostAsyncCommand")]
        public async Task<HandlerResponse> PostAsyncCommand(CommandInfo info)
        {
            var cmd = new ExampleAsyncCommand(info);
            return await _messagingService.PublishAsync(cmd);
        }

        /// <summary>
        /// This example shows how to declare a message handler that will be called
        /// for any message types derived from the handler's declared parameter type.
        /// The parameter is marked with the IncludeDerivedMessages attribute.
        /// </summary>
        [HttpPost Route("derived-event", Name = "PostSyncDerivedEvent")]
        public async Task<IDictionary<string, object>> PostSyncDerivedEvent()
        {
            var evt = new ExampleDerivedDomainEvent();
            await _messagingService.PublishAsync(evt);
            return evt.AttributeValues;
        }

        /// <summary>
        /// This example shows have derived MessageDispatchRule types can be added to
        /// a message handler to determine if it should be invoked based on the state of
        /// the message.
        /// </summary>
        [HttpPost Route("rule-event", Name = "PostEventWithRule")]
        public async Task<IDictionary<string, object>> PostEventWithRule(MessageRuleInfo info)
        {
            var evt = new ExampleRuleDomainEvent(info);
            await _messagingService.PublishAsync(evt);
            return evt.AttributeValues;
        }
    }
}