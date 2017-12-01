using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Testing.Logging
{
    /// <summary>
    /// Instance of a logger used for testing.
    /// </summary>
    public class TestLogger : ILogger,
        IDisposable
    {
        private List<LogEntry> _entries = new List<LogEntry>();

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            if (eventId.Id == 0 )
            {
                return;
            }

            _entries.Add(new LogEntry { EventId = eventId.Id, Level = logLevel , Exception = exception});
        }

        public bool HasSingleEntryFor(int eventId, LogLevel level)
        {
            return _entries.Count(e => e.EventId == eventId && e.Level == level) == 1;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
            _entries.Clear();
        }
    }
}
