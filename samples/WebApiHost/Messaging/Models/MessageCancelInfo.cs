namespace WebApiHost.Messaging.Models
{
    public class MessageCancelInfo
    {
        public int DelayInSeconds { get; set; }
        public int CancelationInSeconds { get; set; }
    }
}
