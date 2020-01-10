using System.Collections.Generic;

namespace NetFusion.Rest.Server.Linking
{
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
        internal string RelationName { get; set; }       
        internal string Href { get; set; }
        internal string HrefLang { get; set; }
        internal IEnumerable<string> Methods { get; set; }

        internal string Name { get; set; }
        internal string Title { get; set; }
        internal string Type { get; set; }
    }
}