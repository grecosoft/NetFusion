using System.Collections.Generic;

namespace NetFusion.Logging
{
    public interface ICompositeLogger
    {
        void Log(IDictionary<string, object> log);
    }
}
