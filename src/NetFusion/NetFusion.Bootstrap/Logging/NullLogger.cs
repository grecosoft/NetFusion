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

        public bool IsDebugLevel
        {
            get { return false; }
        }

        public bool IsVerboseLevel
        {
            get { return false; }
        }

        public void Verbose(string message)
        {
            _messages.Add($"VERBOSE: {message}");
        }

        public void Debug(string message)
        {
            _messages.Add($"DEBUG: {message}");
        }

        public void Warning(string message)
        {
            _messages.Add($"WARNING: {message}");
        }

        public void Error(string message)
        {
            _messages.Add($"ERROR: {message}");
        }
    }
}
