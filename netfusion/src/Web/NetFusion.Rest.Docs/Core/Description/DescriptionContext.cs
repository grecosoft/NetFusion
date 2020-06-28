using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Core.Description
{
    public class DescriptionContext
    {
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    }
}