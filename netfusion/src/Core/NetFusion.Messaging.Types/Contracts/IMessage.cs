using NetFusion.Base.Entity;

namespace NetFusion.Messaging.Types.Contracts
{
    /// <summary>
    /// Interface representing communication between a publisher and consumer.
    /// The message can also be attributed with dynamic properties.
    /// </summary>
    public interface IMessage : IAttributedEntity
    {
    }
}
