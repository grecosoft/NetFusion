using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NetFusion.Messaging.Logging;
using Solution.Context.WebApi.Models;

namespace Solution.Context.WebApi.Hubs
{
    /// <summary>
    /// Implementation of IMessageLogSink delegating all received
    /// message logs to a SignalR Hub.
    /// </summary>
    public class HubMessageLogSink : IMessageLogSink
    {
        private readonly IHubContext<MessageLogHub, IMessageLogHub> _hubContext;
        
        public HubMessageLogSink(IHubContext<MessageLogHub, IMessageLogHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }
        
        public Task WriteLogAsync(MessageLog messageLog)
        {
            return _hubContext.Clients.All.LogMessage(MessageLogModel.FromEntity(messageLog));
        }
    }
}