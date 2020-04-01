using System.Threading.Tasks;
using Subscriber.WebApi.Models;

namespace Subscriber.WebApi.Hubs
{
    public interface IMessageLogHub
    {
        Task LogMessage(MessageLogModel messageLog);
    }
}