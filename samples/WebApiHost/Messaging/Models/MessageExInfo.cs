using NetFusion.Messaging;

namespace WebApiHost.Messaging.Models
{
    public class MessageExInfo
    {
        public int DelayInSeconds = 5;
        public bool ThrowEx = true;
    }
}
