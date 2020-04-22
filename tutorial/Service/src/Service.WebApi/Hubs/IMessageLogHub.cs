using System.Threading.Tasks;
using Service.WebApi.Models;

namespace Service.WebApi.Hubs
{
    public interface IMessageLogHub
    {
        Task LogMessage(MessageLogModel messageLog);
    }
}