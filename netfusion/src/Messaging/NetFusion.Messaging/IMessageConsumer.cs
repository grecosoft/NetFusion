namespace NetFusion.Messaging
{
    /// <summary>
    /// Identifies component as a message consumer that should be searched when messages are published.
    /// This search happens during the bootstrapping of the plug-in and all found components are registered
    /// with the service-collection as scoped components.
    /// </summary>
    public interface IMessageConsumer;
}
