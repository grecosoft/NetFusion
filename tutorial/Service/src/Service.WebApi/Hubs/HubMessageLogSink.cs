using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NetFusion.Messaging.Logging;
using Service.WebApi.Models;

namespace Service.WebApi.Hubs
{
    public class HubMessageLogSink : IMessageLogSink
    {
        private readonly IHubContext<MessageLogHub, IMessageLogHub> _hubContext;
        
        public HubMessageLogSink(IHubContext<MessageLogHub, IMessageLogHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public Task ReceiveAsync(MessageLog messageLog)
        {
            return _hubContext.Clients.All.LogMessage(MessageLogModel.FromEntity(messageLog));
        }
    }
}