using System.Collections.Generic;
using System.Threading.Tasks;
using NetFusion.Messaging.Logging;

namespace CoreTests.Messaging.Logging.Mocks
{
    public class MockLoggingSink : IMessageLogSink
    {
        private readonly List<MessageLog> _receivedLogs = new();

        public MockLoggingSink()
        {
            ReceivedLogs = _receivedLogs;
        }
        
        public IReadOnlyCollection<MessageLog> ReceivedLogs { get; }
        
        public Task WriteLogAsync(MessageLog messageLog)
        {
            _receivedLogs.Add(messageLog);
            return Task.CompletedTask;
        }
    }
}