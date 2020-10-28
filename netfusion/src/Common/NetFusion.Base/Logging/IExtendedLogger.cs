using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Interface implemented to provide extended logging to the base Microsoft ILogger.
    /// A reference can be accessed using NfExtensions.Logger static class property.
    /// </summary>
    public interface IExtendedLogger
    {
        /// <summary>
        /// Writes log message containing a set of detailed properties.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void Write<TContext>(LogMessage message);
        
        /// <summary>
        /// Writes multiple messages containing sets of detailed properties.
        /// </summary>
        /// <param name="messages">Messages to write to the log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public void Write<TContext>(params LogMessage[] messages);
        
        /// <summary>
        /// Writes a list of log messages containing sets of detailed properties.
        /// </summary>
        /// <param name="messages">Messages to write to the log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void Write<TContext>(IEnumerable<LogMessage> messages);
        
        /// <summary>
        /// Writes log message containing options set of arguments.
        /// </summary>
        /// <param name="logLevel">Associated log level.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void Write<TContext>(LogLevel logLevel, string message, 
            params object[] args);
        
        /// <summary>
        /// Writes log message containing set of arguments and a detailed child log property.
        /// </summary>
        /// <param name="logLevel">Associated log level.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="details">Details stored as a log property.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void WriteDetails<TContext>(LogLevel logLevel, string message, object details, 
            params object[] args);

        /// <summary>
        /// Writes an error message containing set of arguments for an exception.
        /// </summary>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void Error<TContext>(Exception ex, string message, 
            params object[] args);

        /// <summary>
        /// Writes an error message containing set of arguments for an exception.
        /// </summary>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="details">Details stored as a log property.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void ErrorDetails<TContext>(Exception ex, string message, object details, 
            params object[] args);
        
        /// <summary>
        /// Writes an error message containing set of arguments for an exception.
        /// </summary>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="details">Details stored as a log property.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        void Error<TContext>(NetFusionException ex, string message, 
            params object[] args);
    }
}