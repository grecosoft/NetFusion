namespace NetFusion.Web.Rest.Docs.Models;

/// <summary>
/// Model representing the status code returned from a Web Api method a
/// nd the optional associated response resource.
/// </summary>
public class ApiResponseDoc
{
    /// <summary>
    /// The HTTP response status code associated with the response.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// The documentation associated with the resource that will
    /// be returned for the status code.  This property will be
    /// null if a resource is not returned for the status code.
    /// </summary>
    public ApiResourceDoc ResourceDoc { get; set; } 
}