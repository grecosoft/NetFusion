using NetFusion.Rest.Resources;

namespace WebTests.Rest.Resources.Models
{
    [Resource("ReminderResource")]
    public class ReminderModel
    {
        public string Message { get; set; }
    }
}