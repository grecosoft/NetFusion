namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Strategy invoked when the microservice is stopped used
/// to dispose a created service-bus entity. 
/// </summary>
public interface IBusEntityDisposeStrategy : IBusEntityStrategy
{
    /// <summary>
    /// Called when the entity should be disposed.
    /// </summary>
    /// <returns>Future Result.</returns>
    Task OnDispose();
}