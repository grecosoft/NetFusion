namespace Demo.WebApi
{
    using System.Threading.Tasks;

    public interface ITestSubscriptionService
    {
        Task AddSubscription(string channel);
        Task RemoveSubscription(string channel);
    }
}