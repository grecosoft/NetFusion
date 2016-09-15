using NetFusion.Common.Extensions;
using System;
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

        public bool IsVerboseLevel => true;
        public bool IsInfoLevel => true;
        public bool IsDebugLevel => true;
        public bool IsWarningLevel => true;
        public bool IsErrorLevel => true;
    
        public IContainerLogger ForContext<TContext>()
        {
            return this;
        }

        public IContainerLogger ForContext(Type contextType)
        {
            return this;
        }

        private void AddToMessageFor(string level, string message, 
            object details, Exception ex = null)
        {
            details = details ?? new { };
            string exMsg = ex?.Message ?? "";
            _messages.Add($"{level}:{message}; Details{details.ToIndentedJson()}; Exception: {exMsg}");
        }

        public void Verbose(string message, object details = null)
        {
            AddToMessageFor("VERBOSE", message, details);
        }

        public void Info(string message, object details = null)
        {
            AddToMessageFor("INFO", message, details);
        }

        public void Debug(string message, object details = null)
        {
            AddToMessageFor("DEBUG", message, details);
        }

        public void Warning(string message, object details = null)
        {
            AddToMessageFor("WARNING", message, details);
        }

        public void Error(string message, object details = null)
        {
            AddToMessageFor("ERROR", message, details);
        }

        public void Error(string message, Exception ex, object details = null)
        {
            AddToMessageFor("VERBOSE", message, details, ex);
        }


    }
}
