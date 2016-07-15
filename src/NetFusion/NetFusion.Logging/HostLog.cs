using System.Collections.Generic;

namespace NetFusion.Logging
{
    public class HostLog
    {
        public HostLog(
            string name, 
            IDictionary<string, object> log)
        {
            this.Name = name;
            this.Log = log;
        }

        public string Name { get; }
        public IDictionary<string, object> Log { get; }
    }
}
