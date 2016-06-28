using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Messages
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
