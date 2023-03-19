namespace NetFusion.Integration.Bus.Strategies;

public interface IBusEntitySubscriptionStrategy : IBusEntityStrategy
{
    Task SubscribeEntity();
}