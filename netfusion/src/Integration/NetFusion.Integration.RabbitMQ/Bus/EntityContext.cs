using EasyNetQ;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Plugin;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Bus;

/// <summary>
/// Context specific to RabbitMq bus implementation.
/// </summary>
public class EntityContext : BusEntityContext
{
    public IBusModule BusModule { get; }
    
    public EntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider) : 
        base(hostPlugin, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        BusModule = serviceProvider.GetRequiredService<IBusModule>();
    }

    public bool IsAutoCreateEnabled => BusModule.RabbitMqConfig.IsAutoCreateEnabled;

    public MessageProperties GetMessageProperties(IMessage message)
    {
        var props = new MessageProperties
        {
            AppId = HostPlugin.PluginId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
            
        string? correlationId = message.GetCorrelationId();
        if (correlationId != null)
        {
            props.CorrelationId = correlationId;
        }
            
        string? messageId = message.GetMessageId();
        if (messageId != null)
        {
            props.MessageId = messageId;
        }

        byte? msgPriority = message.GetPriority();
        if (msgPriority != null)
        {
            props.Priority = msgPriority.Value;
        }

        return props;
    }
    
    public void SetMessageProperties(MessageProperties msgProps, IMessage message)
    {
        if (msgProps.CorrelationIdPresent)
        {
            message.SetCorrelationId(msgProps.CorrelationId);
        }

        if (msgProps.MessageIdPresent)
        {
            message.SetMessageId(msgProps.MessageId);
        }

        if (msgProps.ReplyToPresent)
        {
            message.SetReplyTo(msgProps.ReplyTo);
        }

        if (msgProps.ContentTypePresent)
        {
            message.SetContentType(msgProps.ContentType);
        }
    }
}