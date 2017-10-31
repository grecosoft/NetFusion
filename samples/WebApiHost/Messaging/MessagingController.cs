using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Web.Mvc.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WebApiHost.Messaging.Messages;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging
{
    [Route("api/[controller]")]
    [GroupMeta]
    public class MessagingController : Controller
    {
        private IMessagingService _messagingSrv;

        public MessagingController(IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        /// <summary>
        /// This example shows publishing a domain event to two synchronous handlers. 
        /// Each handler method puts the current thread to sleep for the specified
        /// number of seconds.  There are two handlers for this event so the time
        /// to execute will be 2 x specified-seconds.
        /// </summary>
        [HttpPost("sync-event"), ActionMeta("PublishDomainEventSync")]
        public async Task<MessageResponse> PublishDomainEventSync([FromBody]MessageInfo info)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var evt = new ExampleSyncDomainEvent(info);
            await _messagingSrv.PublishAsync(evt);

            stopwatch.Stop();
            return new MessageResponse { Elapsed = stopwatch.Elapsed };
        }

        /// <summary>
        /// This example shows publishing a domain event to two asynchronous handlers.  Each handler 
        /// creates a new task that puts the current thread to sleep for the specified number of seconds.  
        /// There are two handlers for this event that are asynchronously executed so the time should be
        /// about 1 x specified-seconds.
        /// </summary>
        [HttpPost("async-event"), ActionMeta("PublishDomainEventAsync")]
        public async Task<MessageResponse> PublishDomainEventAsync([FromBody]MessageInfo info)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var evt = new ExampleAsyncDomainEvent(info);
            await _messagingSrv.PublishAsync(evt);

            stopwatch.Stop();
            return new MessageResponse { Elapsed = stopwatch.Elapsed };
        }

        [HttpPost("async-cancel-event"), ActionMeta("PublishDomainEventAsync")]
        public async Task<MessageResponse> PublishDomainCancelEventAsync([FromBody]MessageCancelInfo info)
        {
            if (info.CancelationInSeconds == 0)
            {
                info.CancelationInSeconds += info.DelayInSeconds + 1000;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(info.CancelationInSeconds * 1000);

            var evt = new ExampleAsyncCancelEvent(info);
            await _messagingSrv.PublishAsync(evt, cts.Token);

            stopwatch.Stop();
            return new MessageResponse { Elapsed = stopwatch.Elapsed };
        }

        /// <summary>
        /// This example publishes an event that is handled by two asynchronous handlers
        /// that each throw an exception.  The details of the PublisherException are
        /// returned to the calling client.
        /// </summary>
        [HttpPost("async-exception"), ActionMeta("PostAsyncEventException")]
        public async Task<string> PostAsyncEventException([FromBody]MessageExInfo info)
        {
            var evt = new ExampleAsyncDomainEventException(info);

            try
            {
                await _messagingSrv.PublishAsync(evt);
            }
            catch (PublisherException ex)
            {
                return ex.Details.ToIndentedJson();
            }

            return "No Exception";
        }

        /// <summary>
        /// This example publishes a command with a typed result.  A command 
        /// message type can have one and only one handler.  If more than one
        /// handler is found, an exception will be thrown during the bootstrap
        /// process.
        /// </summary>
        [HttpPost("async-command"), ActionMeta("PostAsyncCommand")]
        public Task<HandlerResponse> PostAsyncCommand([FromBody]CommandInfo info)
        {
            var cmd = new ExampleAsyncCommand(info);
            return _messagingSrv.SendAsync(cmd);
        }

        /// <summary>
        /// This example shows how to declare a message handler that will be called
        /// for any message types derived from the handler's declared parameter type.
        /// The parameter is marked with the IncludeDerivedMessages attribute.
        /// </summary>
        [HttpPost("derived-event"), ActionMeta("PostSyncDerivedEvent")]
        public async Task<IDictionary<string, object>> PostSyncDerivedEvent()
        {
            var evt = new ExampleDerivedDomainEvent();
            await _messagingSrv.PublishAsync(evt);
            return evt.AttributeValues;
        }

        /// <summary>
        /// This example shows have derived MessageDispatchRule types can be added to
        /// a message handler to determine if it should be invoked based on the state of
        /// the message.
        /// </summary>
        [HttpPost("rule-event"), ActionMeta("PostEventWithRole")]
        public async Task<IDictionary<string, object>> PostEventWithRule([FromBody]MessageRuleInfo info)
        {
            var evt = new ExampleRuleDomainEvent(info);
            await _messagingSrv.PublishAsync(evt);
            return evt.AttributeValues;
        }
    }
}
