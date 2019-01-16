namespace Service.Client.Commands
{
    using NetFusion.Messaging.Types;

    public class SendEmail : Command
    {
        public string Subject { get; set; }
        public string FromAddress { get; set; }
        public string[] ToAddresses { get; set; }
        public string Message { get; set; }
    }
}