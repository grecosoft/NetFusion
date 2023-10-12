namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Strategy invoked during bootstrap to subscribe to a service-bus entity.
/// </summary>
public interface IBusEntitySubscriptionStrategy : IBusEntityStrategy
{
    /// <summary>
    /// Called when the strategy should subscribe a message handler
    /// to be called when a message arrives on the service-bus entity.
    /// </summary>
    /// <returns>Future Result.</returns>
    Task SubscribeEntity();
}