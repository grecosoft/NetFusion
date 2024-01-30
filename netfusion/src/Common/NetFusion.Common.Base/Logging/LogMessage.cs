using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Common.Base.Logging;

/// <summary>
/// Class representing a log message with a set of optional detailed properties.
/// </summary>
public class LogMessage
{
    private readonly List<LogProperty> _logProperties = new();

    private LogMessage(LogLevel logLevel, string message, object[] args)
    {
        Message = message;
        LogLevel = logLevel;
        Args = args;
        Properties = _logProperties.AsReadOnly();
    }
        
    /// <summary>
    /// The log level associated with the message.
    /// </summary>
    public LogLevel LogLevel { get; }
        
    /// <summary>
    /// Log message template containing optional argument values.
    /// </summary>
    public string Message { get; }
        
    /// <summary>
    /// Optional argument values replaced in the message template.
    /// </summary>
    public object[] Args { get; }

    /// <summary>
    /// Set of properties associated with the message used to write
    /// structured event messages.
    /// </summary>
    public IReadOnlyCollection<LogProperty> Properties { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logLevel">The log level associated with the message.</param>
    /// <param name="message">Log message template containing optional argument values.</param>
    /// <param name="args">Optional argument values replaced in the message template.</param>
    /// <returns>Log message instance.</returns>
    public static LogMessage For(LogLevel logLevel, string message, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(message)) 
            throw new ArgumentException("Message not specified.", nameof(message));

        return new LogMessage(logLevel, message, args);
    }

    /// <summary>
    /// Adds list of log properties to the message written as a structured event log.
    /// </summary>
    /// <param name="properties"> Set of detail properties associated with the message.</param>
    /// <returns>Reference to log message.</returns>
    public LogMessage WithProperties(params LogProperty[] properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        _logProperties.AddRange(properties);
        return this;
    }
}