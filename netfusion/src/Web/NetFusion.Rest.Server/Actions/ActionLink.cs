using System.Collections.Generic;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Defines a link with an associated relation name.  The relation
    /// name indicates how the link is associated to the resource.
    /// </summary>
    public class ActionLink
    {
        internal string RelationName { get; set; }       
        internal string Href { get; set; }
        internal string HrefLang { get; set; }
        internal IEnumerable<string> Methods { get; set; }

        internal string Name { get; set; }
        internal string Title { get; set; }
        internal string Type { get; set; }

        internal ActionLink Deprecation { get; set; }
        internal ActionLink Profile { get; set; }
    }
}