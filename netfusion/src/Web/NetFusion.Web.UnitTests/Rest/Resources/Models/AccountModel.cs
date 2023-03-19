using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.UnitTests.Rest.Resources.Models;

/// <summary>
/// Represents a model returned from a REST Api.
/// </summary>
[Resource("AccountResource")]
public class AccountModel
{
    public string AccountNumber { get; set; }
    public decimal AvailableBalance { get; set; }
}