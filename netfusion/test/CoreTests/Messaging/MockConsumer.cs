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
        
        protected MockConsumer()
        {
        }

        protected MockConsumer(IMockTestLog testLog)
        {
            TestLog = testLog;
        }
    }

    public interface IMockTestLog
    {
        IReadOnlyCollection<string> Entries { get; }
        IReadOnlyCollection<IMessage> Messages { get; }
        
        IMockTestLog AddLogEntry(string logMessage);
        IMockTestLog RecordMessage(IMessage message);
    }
    
    public class MockTestLog : IMockTestLog
    {
        private readonly List<string> _entries = new();
        private readonly List<IMessage> _messages = new();
        
        public IReadOnlyCollection<string> Entries { get; }
        public IReadOnlyCollection<IMessage> Messages { get; }
        
        public MockTestLog()
        {
            Entries = _entries;
            Messages = _messages;
        }
        
        public IMockTestLog AddLogEntry(string logMessage)
        {
            _entries.Add(logMessage);
            return this;
        }

        public IMockTestLog RecordMessage(IMessage message)
        {
            _messages.Add(message);
            return this;
        }
    }
}
