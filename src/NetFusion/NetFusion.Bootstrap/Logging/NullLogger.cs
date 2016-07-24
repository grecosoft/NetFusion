using System.Collections.Generic;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Null implementation of the logger used if the host application
    /// does not provided an instance.
    /// </summary>
    public class NullLogger : IContainerLogger
    {
        private List<string> _messages = new List<string>();

        public IEnumerable<string> Messages
        {
            get { return _messages; }
        }

        public bool IsVerboseLevel => false;
        public bool IsInfoLevel => false;
        public bool IsDebugLevel => false;
        public bool IsWarningLevel => false;
        public bool IsErrorLevel => false;
    
        public IContainerLogger ForContext<TContext>()
        {
            return this;
        }

        public void Verbose(string message, object details = null)
        {
            _messages.Add($"VERBOSE: {message}");
        }

        public void Info(string message, object details = null)
        {
            _messages.Add($"INFO: {message}");
        }

        public void Debug(string message, object details = null)
        {
            _messages.Add($"DEBUG: {message}");
        }

        public void Warning(string message, object details = null)
        {
            _messages.Add($"WARNING: {message}");
        }

        public void Error(string message, object details = null)
        {
            _messages.Add($"ERROR: {message}");
        }   
    }
}
