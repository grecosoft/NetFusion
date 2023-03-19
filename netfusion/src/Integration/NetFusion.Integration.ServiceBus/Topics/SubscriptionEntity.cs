using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Topics.Metadata;
using NetFusion.Integration.ServiceBus.Topics.Strategies;

namespace NetFusion.Integration.ServiceBus.Topics;

/// <summary>
/// Defines a subscription to a topic, defined by another microservice,
/// to receive notifications of changes. 
/// </summary>
internal class SubscriptionEntity : BusEntity
{
    public string TopicName { get; }
    public SubscriptionMeta SubscriptionMeta { get; }
    private readonly MessageDispatcher _messageDispatcher;
    
    public SubscriptionEntity(string namespaceName, string subscriptionName,  string topicName, 
        MessageDispatcher dispatcher) : base(namespaceName, subscriptionName)
    {
        TopicName = topicName;
        
        SubscriptionMeta = dispatcher.RouteMeta as SubscriptionMeta ?? 
                           throw new NullReferenceException("Subscription metadata not specified");
        
        _messageDispatcher = dispatcher;
        
        AddStrategies(new TopicSubscriptionStrategy(this, dispatcher));
    }

    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { _messageDispatcher };

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "CommandType" , _messageDispatcher.MessageType.Name },
        { "Subscription" , EntityName },
        { "BusName", BusName },
        { "Topic" , TopicName },
        { "Consumer" , _messageDispatcher.ConsumerType.Name },
        { "Handler" , _messageDispatcher.MessageHandlerMethod.Name },
        { "Roles" , string.Join(", ", SubscriptionMeta.RuleOptions
            .Select(r => $"Name: {r.Name} Filer: {r.Filter} Action: {r.Action}").ToArray()) }
    };
}