using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface IDocDescription
    {
        IDictionary<string, object> Context { get; set; }
    }
}