using System.Collections.Generic;

namespace NetFusion.Common
{
    public interface IObjectAttributes
    {
        IDictionary<string, object> Attributes { get; }
    }
}
