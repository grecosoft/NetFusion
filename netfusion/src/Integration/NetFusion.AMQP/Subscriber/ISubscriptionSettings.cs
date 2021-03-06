using System.Threading.Tasks;

namespace NetFusion.AMQP.Subscriber
{
    /// <summary>
    /// Interface implemented by the consuming host application that
    /// provides a mapping between Host Item and Subscription names to a
    /// corresponding application create subscription.
    /// </summary>
    public interface ISubscriptionSettings
    {
        /// <summary>
        /// Implementor should return the name of a mapped subscription 
        /// if configured.  If not configured, it should return null.
        /// </summary>
        /// <param name="mapping">The mapping settings.</param>
        /// <returns>The mapped subscription name or null if not found.</returns>
        string GetMappedSubscription(SubscriptionMapping mapping);

        /// <summary>
        /// Called to allow settings to be configured.
        /// </summary>
        /// <returns>Task.</returns>
        Task ConfigureSettings();

        /// <summary>
        /// Called to allow settings to be deleted if they should not
        /// persist between host executions.
        /// </summary>
        /// <returns>Task.</returns>
        Task CleanupSettings();
    }
}