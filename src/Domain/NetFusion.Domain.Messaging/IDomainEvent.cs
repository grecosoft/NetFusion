namespace NetFusion.Domain.Messaging
{
    /// <summary>
    /// Identifies a type as a message that is used to notify
    /// consumers that and event has occurred. 
    /// </summary>
    public interface IDomainEvent : IMessage
    {

    }
}
