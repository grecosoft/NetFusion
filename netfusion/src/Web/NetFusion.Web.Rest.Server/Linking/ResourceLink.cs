namespace NetFusion.Web.Rest.Server.Linking;

/// <summary>
/// Defines a link with an associated relation name.  The relation name indicates how
/// the link is associated to the resource.  For example, the resource is a customer,
/// there could be an associated resource-links with the relation names of:
///     - self:  The URL that can be used to reload the resource.
///     - mailing-address:  The URL used to load the customer's mailing address.
///     - pending-orders:  The URL used to load the customer's pending orders.
/// </summary>
public class ResourceLink
{
    public string RelationName { get; }

    public ResourceLink(string relationName)
    {
        RelationName = relationName;
    }
        
    public string? Href { get; set; }
    public string Method { get; set; }
        
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? HrefLang { get; set; }
}