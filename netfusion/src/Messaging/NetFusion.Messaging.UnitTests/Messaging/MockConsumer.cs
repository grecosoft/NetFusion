﻿namespace NetFusion.Messaging.UnitTests.Messaging;

/// <summary>
/// Message consumer that records the called message handler methods.
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
    IReadOnlyCollection<object> Messages { get; }
        
    IMockTestLog AddLogEntry(string logMessage);
    IMockTestLog RecordMessage(object message);
}
    
public class MockTestLog : IMockTestLog
{
    private readonly List<string> _entries = [];
    private readonly List<object> _messages = [];
        
    public IReadOnlyCollection<string> Entries { get; }
    public IReadOnlyCollection<object> Messages { get; }
        
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

    public IMockTestLog RecordMessage(object message)
    {
        _messages.Add(message);
        return this;
    }
}