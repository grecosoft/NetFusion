using NetFusion.Messaging.Types;

namespace Demo.Client.Commands
{
    public class SendEmail : Command
    {
        public string Subject { get; set; }
        public string FromAddress { get; set; }
        public string[] ToAddresses { get; set; }
        public string Message { get; set; }
    }
}