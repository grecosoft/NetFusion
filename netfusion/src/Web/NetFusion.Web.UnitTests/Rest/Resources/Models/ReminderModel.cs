using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.UnitTests.Rest.Resources.Models;

/// <summary>
/// Represents a model returned from a REST Api.
/// </summary>
[Resource("ReminderResource")]
public class ReminderModel
{
    public string Message { get; set; }
}