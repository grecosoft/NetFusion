using NetFusion.Domain.Messaging;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging.Messages
{
    public class ExampleAsyncCommand : Command<HandlerResponse>
    {
        public string Message { get; set; }
        public int Seconds { get; set; }

        public ExampleAsyncCommand(CommandInfo info)
        {
            this.Message = info.InputMessage;
            this.Seconds = info.Seconds;
        }
    }
}
