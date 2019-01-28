namespace NetFusion.Azure.Messaging.Subscriber
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface implemented by the consuming host application that
    /// provides a mapping between Topic and Subscription names to a
    /// corresponding application create subscription.
    /// </summary>
    public interface ISubscriptionSettings
    {
        /// <summary>
        /// Implementor should return the name of a mapping topic subscription if
        /// configured.  If not configured, it should return null.
        /// </summary>
        /// <param name="namespaceName">The namespace defining the topic.</param>
        /// <param name="mapping">The mapping settings.</param>
        /// <returns>The mapped subscription name or null if not found.</returns>
        string GetMappedSubscription(string namespaceName, SubscriptionMapping mapping);

        /// <summary>
        /// Called to allow settings to be configured.
        /// </summary>
        /// <returns>Task</returns>
        Task ConfigureSettings();
    }
}