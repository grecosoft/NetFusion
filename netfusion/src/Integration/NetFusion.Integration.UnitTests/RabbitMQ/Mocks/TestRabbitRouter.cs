using EasyNetQ.Topology;
using NetFusion.Common.Base;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class TestRabbitRouter() : RabbitMqRouterBase("testRabbitBus")
{
    public IEnumerable<BusEntity> DefinedEntities
    {
        get
        {
            OnDefineEntities();
            return BusEntities;
        }
    }

    protected override void OnDefineEntities()
    {
        // Example routing specifying an exchange a domain-event
        // is delivered when published.
        DefineExchange<TestDomainEvent> (exchange =>
        {
            exchange.ExchangeName = "TestExchange";
            exchange.ExchangeType = ExchangeType.Topic;
            exchange.AlternateExchangeName = "AltTestExchange";
            exchange.IsDurable = true;
            exchange.IsAutoDelete = true;
            exchange.PublishOptions.Priority = 2;
            exchange.PublishOptions.ContentType = ContentTypes.MessagePack;
            exchange.RouteKey(e => $"{e.ValueOne}.{e.ValueTwo}");
            exchange.WhenDomainEvent(e => e.ValueTwo < 250);
        });
        
        // Example routing specifying the consumer a domain-event is
        // dispatched to when received on exchange.
        SubscribeToExchange<TestDomainEvent>("TestExchange", ["10.20", "50.100"], route =>
        {
            route.ToConsumer<TestDomainEventHandler>(c => c.OnDomainEvent, meta =>
            {
                meta.QueueName = "ReceivedTestExchangeEvents";
                meta.Priority = 2;
                meta.IsDurable = true;
                meta.IsAutoDelete = true;
                meta.IsExclusive = true;
                meta.MaxPriority = 5;
                meta.PrefetchCount = 4;
                meta.DeadLetterExchangeName = "DeadExchangeName";
            });
        });
        
        // A microservice can define a queue to which other services
        // can send commands for processing.
        DefineQueue<TestCommand>(route =>
        {
            route.ToConsumer<TestCommandHandler>(c => c.OnCommand, queue =>
            {
                queue.QueueName = "TestQueue";
                queue.Priority = 1;
                queue.IsDurable = true;
                queue.IsExclusive = true;
                queue.IsAutoDelete = true;
                queue.MaxPriority = 10;
                queue.PrefetchCount = 5;
                queue.DeadLetterExchangeName = "DeadExchangeName";
            });
        });
        
        // A microservice can specify the queue owned by another service
        // a command should be delivered when sent.
        RouteToQueue<TestCommand>("TestQueue", options =>
        {
            options.Priority = 1;
            options.ContentType = ContentTypes.MessagePack;
            options.IsMandatory = true;
            options.IsPersistent = true;
        });
        
        // A microservice can define a queue to which other services 
        // can send commands for processing expecting a response.
        DefineQueueWithResponse<TestCommandWithResponse, TestCommandResponse>(route =>
        {
            route.ToConsumer<TestCommandHandlerWithResponse>(c => c.OnCommand,
                queue =>
                {
                    queue.QueueName = "TestQueueWithResponse";
                });
        });
        
        // A microservice can specify the queue to which a command can be
        // sent and the reply-queue on which the response should be delivered.
        RouteToQueueWithResponse<TestCommandWithResponse, TestCommandResponse>("TestQueueWithResponse",
            route => route.ToConsumer<TestReplyQueueHandler>(c => c.OnReply, replyQueue =>
            {
                replyQueue.QueueName = "TestCommandReplyQueue";
            }));
    }
}