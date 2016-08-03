using NetFusion.Messaging;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Messages.RabbitMQ;
using RefArch.Api.Models;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    /// <summary>
    /// Examples of publishing messages that are sent to RabbitMQ by 
    /// registering RabbitMqMessagePublisher.  The following is the
    /// needed configuration:
    /// 
    /// 
    /// .WithConfig((MessagingConfig config) =>
    /// {
    ///     config.AddMessagePublisherType<RabbitMqMessagePublisher>();
    /// })
    /// 
    /// One of the registered settings initializers need to be able to
    /// provide the needed broker settings.  This example is using the
    /// FileSettingsInitializer and the configuration file is:
    /// Configs/Dev/BrokerSettings.json
    /// 
    /// After the above is configured, the following are the steps for
    /// defining a message, exchange, queue, and consumer.
    /// 
    /// 1.  Define a derived message type.
    /// 2.  Define an exchange with optional queues.
    /// 3.  Define the consumer.
    /// 4.  Publish the message using the common IMessagingService interface.
    /// 
    /// </summary>
    [EndpointMetadata(EndpointName = "NetFusion.RabbitMQ", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/rabbitmq")]
    public class RabbitMQController : ApiController
    {
        private IMessagingService _messagingSrv;

        public RabbitMQController(
            IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------
        /// DIRECT EXCHANGE PATTERN
        /// ---------------------------------------------------------------------------------------------------------
        /// - The direct exchange uses the route key to determine which queues should receive the 
        ///   message.  When a queue is bound to an exchange, it can specify a route key value.
        ///     
        /// - When messages are published to a direct exchange, the publisher specifies a route key 
        ///   value.  The exchange will deliver the message to all queues that have a binding with 
        ///   the specified route key value.
        ///     
        /// - A given queue to be bound more than once to an exchange - each binding using a different 
        ///   route key.  If a message's routing key does not match any of the queue bindings, the message
        ///   is discarded.
        /// 
        /// - It is perfectly legal to bind multiple queues with the same binding key.  In that case, the 
        ///   direct exchange will behave like fanout and will broadcast the message to all the matching
        ///   queues. 
        ///   
        /// Example Declarations:
        /// Message:  <see cref="RefArch.Api.Messages.ExampleDirectEvent"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.ExampleDirectExchange"/>
        /// Consumer: <see>RefArch.Subscriber.Services.ExampleDirectService</see>
        /// </summary>
        /// <param name="car">Submitted Request.</param>
        /// <returns>Void</returns>
        [HttpPost, Route("direct", Name = "PublishDirectEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishDirectEvent(Car car)
        {
            var evt = new ExampleDirectEvent(car);
            evt.Attributes.Values.Test = "AAAABBBBB";
            await _messagingSrv.PublishAsync(evt);
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------
        /// TOPIC EXCHANGE PATTERN
        /// ---------------------------------------------------------------------------------------------------------
        /// - The same as a direct exchange.However, the route key value is a filter and not 
        ///   just a value.
        ///   
        /// - The route key used to specify a queue to exchange binding consists of a filter with 
        ///   '.' delimited values:  A.B.*
        /// 
        /// - When a message is posted, the message will only be delivered to the queue if one its 
        ///   binding filter values match the posted route key value.
        ///   
        /// Example Declarations:
        /// Message:  <see cref="RefArch.Api.Messages.ExampleTopicEvent"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.ExampleTopicExchange"/>
        /// Consumer: <see>RefArch.Subscriber.Services.ExampleTopicService</see>
        /// </summary>
        /// <param name="car">Submitted Request.</param>
        /// <returns>Void</returns>
        [HttpPost, Route("topic", Name = "PublishTopicEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishTopicEvent(Car car)
        {
            var evt = new ExampleTopicEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------
        /// FANOUT EXCHANGE PATTERN
        /// --------------------------------------------------------------------------------------------------------- 
        /// - This scenario uses exchange which is responsible for determining and delivering messages to queues.
        /// - An exchange of type Fan-out will broadcast a message to all queues defined on the exchange.
        /// - This type of configuration is often used when the message needs to be sent to many receivers.
        /// - This configuration usually does not require the message to be acknowledged by the consumers.
        /// - This setup is achieved by having each receiver define an queue that is bound to by the consumer and
        ///   automatically deleted once the consumer disconnects.
        ///
        /// - Route keys are not used by this type of exchange.
        ///
        /// - Giving a queue a name is important when you want to share the queue between producers and consumers.
        ///     But in this case, the publisher usually does not know about the consumers.  In this case a temporary
        ///     named queue is best.  Also, the consumer is usually not interested in old messages, just current ones.
        ///
        /// - Firstly, whenever we connect to Rabbit we need a fresh, empty queue. To do this we could create a queue
        ///   with a random name, or, even better - let the server choose a random queue name for us.  Secondly, once 
        ///   we disconnect the consumer the queue should be automatically deleted.
        ///
        /// - The messages will be lost if no queue is bound to the exchange yet.  For most publish/subscribe scenarios
        ///   this is what we would want. 
        ///   
        /// Example Declarations:
        /// Message:  <see cref="RefArch.Api.Messages.ExampleFanoutEvent"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.AmericanCarExchange"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.GermanCarExchange"/>
        /// Consumer: <see>RefArch.Subscriber.Services.ExampleFanoutService</see>
        /// </summary>
        /// <param name="car">Submitted Request.</param>
        /// <returns>Void</returns>
        /// 
        [HttpPost, Route("fanout", Name = "PublishFanoutEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishFanoutEvent(Car car)
        {
            var evt = new ExampleFanoutEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------
        /// WORK-QUEUE EXCHANGE PATTERN
        /// ---------------------------------------------------------------------------------------------------------
        /// - Used to distribute tasks published to the queue to multiple consumers in a round-robin fashion.
        /// - Publisher message RouteKey == Queue Name.
        /// - When configuring a work-flow queue, it is defined using the default exchange.
        /// - Consumers bind to a work flow queue by using the name assigned to the queue.
        /// - Publishers publish messages by specifying the name of queue as the RouteKey.
        /// 
        /// - The message will be delivered to the queue having the same name as the route key and delivered
        ///     to bound consumers in a round robin sequence.
        /// 
        /// - This type of queue is used to distribute time intensive tasks to multiple consumers bound to the queue.
        /// - The tasks may take several seconds to complete.  When the consumer is processing the task and fails,
        ///     another bound consumer should be given the task.  This is achieved by having the client acknowledge
        ///     the task once its processing has completed.
        /// 
        /// - There aren't any message timeouts; RabbitMQ will redeliver the message only when the worker connection dies.
        ///   It's fine even if processing a message takes a very, very long time.
        /// 
        /// - For this type of queue, it is usually desirable to not loose the task messages if the RabbitMQ server is
        ///     restarted or would crash.  Two things are required to make sure that messages aren't lost: we need to 
        ///     mark both the queue and messages as durable.
        /// 
        /// - If fair dispatch is enabled, RabbitMQ will not dispatch a message to a consumer if there is a pending
        ///   acknowlegement.This keeps a busy consumer from getting a backlog of messages to process.
        /// 
        /// - If all the workers are busy, your queue can fill up.  You will want to keep an eye on that, and maybe add
        ///   more workers, or have some other strategy.
        ///   
        /// Example Declarations:
        /// Message:  <see cref="RefArch.Api.Messages.ExampleWorkQueueEvent"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.ExampleWorkQueueExchange"/>
        /// Consumer: <see>RefArch.Subscriber.Services.ExampleWorkQueueService</see>
        /// </summary>
        /// <param name="car">Submitted Request.</param>
        /// <returns>Void</returns>
        [HttpPost, Route("work-queue", Name = "PublishWorkQueueEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishWorkQueueEvent(Car car)
        {
            var evt = new ExampleWorkQueueEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }

        /// <summary>
        /// ---------------------------------------------------------------------------------------------------------
        /// RPC EXCHANGE PATTERN
        /// ---------------------------------------------------------------------------------------------------------
        /// The message that is published for this exchange pattern must be a command.  With this pattern, the 
        /// publisher publishes the command and the subscriber returns a response.
        /// 
        /// Example Declarations:
        /// Message:  <see cref="RefArch.Api.Messages.ExampleRpcCommand"/>
        /// Response: <see cref="RefArch.Api.Messages.ExampleRpcResponse"/>
        /// Exchange: <see cref="RefArch.Infrastructure.Samples.RabbitMQ.ExampleRpcExchange"/>
        /// Consumer: <see>RefArch.Subscriber.Services.ExampleRpcService</see>
        /// </summary>
        /// <param name="car">Submitted Request.</param>
        /// <returns>The message consumer's response.</returns>
        [HttpPost, Route("rpc", Name = "PublishRPCEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task<ExampleRpcResponse> PublishRPCEvent(Car car)
        {
            var evt = new ExampleRpcCommand(car);
            return await _messagingSrv.PublishAsync(evt);
        }
    }
}

