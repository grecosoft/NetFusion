using System.Collections.Generic;

namespace NetFusion.Base.Logging
{
    public class LogMessage
    {
        private LogMessage() { }
        
        public string Message { get; private set; }
        public object[] Args { get; private set; }
        public List<LogProperty> Properties { get; } = new List<LogProperty>();

        public static LogMessage For(string message, params object[] args)
        {
            return new LogMessage
            {
                Message = message,
                Args = args
            };
        }

        public LogMessage WithProperties(params LogProperty[] details)
        {
            Properties.AddRange(details);
            return this;
        }
    }
}