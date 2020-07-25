using NetFusion.Rest.Resources;

namespace WebTests.Rest.Resources.Models
{
    /// <summary>
    /// Represents a model returned from a REST Api.
    /// </summary>
    [Resource("ReminderResource")]
    public class ReminderModel
    {
        public string Message { get; set; }
    }
}