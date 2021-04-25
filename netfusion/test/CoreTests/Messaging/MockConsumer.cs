using System.Collections.Generic;
using NetFusion.Messaging.Types.Contracts;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Message consumer that records the called message
    /// handler methods.
    /// </summary>
    public abstract class MockConsumer
    {
        protected IMockTestLog TestLog { get; }
        
        private readonly List<string> _executedHandlers = new ();
        private readonly List<IMessage> _receivedMessages = new();

        protected MockConsumer()
        {
        }

        protected MockConsumer(IMockTestLog testLog)
        {
            TestLog = testLog;
            
            ExecutedHandlers = _executedHandlers;
            ReceivedMessages = _receivedMessages;
        }

        public IReadOnlyCollection<string> ExecutedHandlers { get; }
        public IReadOnlyCollection<IMessage> ReceivedMessages { get; }

        protected void AddCalledHandler(string handlerName)
        {
            TestLog.AddLogEntry(handlerName);
            _executedHandlers.Add(handlerName);
        }

        protected void RecordReceivedMessage(IMessage message) => _receivedMessages.Add(message);
    }

    public interface IMockTestLog
    {
        IReadOnlyCollection<string> Entries { get; }
        IMockTestLog AddLogEntry(string logMessage);
    }
    
    public class MockTestLog : IMockTestLog
    {
        private readonly List<string> _entries = new();
        public IReadOnlyCollection<string> Entries { get; }
        
        public MockTestLog()
        {
            Entries = _entries;
        }
        
        public IMockTestLog AddLogEntry(string logMessage)
        {
            _entries.Add(logMessage);
            return this;
        }
        
    }
}
