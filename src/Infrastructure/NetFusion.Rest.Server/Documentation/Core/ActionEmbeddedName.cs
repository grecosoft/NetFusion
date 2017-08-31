using System;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// The embedded name of a resource and its type specified on an action
    /// method using the CodEmbeddedResource attribute.
    /// </summary>
    public class ActionEmbeddedName
    {
        public string Name { get; internal set; }
        public Type ResourceType { get; internal set; }
    }
}