using NetFusion.Logging.Configs;
using System.Collections.Generic;

namespace NetFusion.Logging.Core
{
    public class CompositeLogger : ICompositeLogger
    {
        private readonly CompositeLogSettings _logSettings;

        public CompositeLogger(CompositeLogSettings logSettings)
        {
            _logSettings = logSettings;
        }

        public void Log(IDictionary<string, object> log)
        {
            if (_logSettings.SendLog)
            {

            }
        }
    }
}
