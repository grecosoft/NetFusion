using System;
using Microsoft.Extensions.Logging;

namespace NetFusion.RabbitMQ.Logging
{
    /// <summary>
    /// Delegates to Microsoft's extensions logger any logs written by EasyNetQ.
    /// </summary>
    internal class RabbitMqLogProvider : EasyNetQ.Logging.ILogProvider
    {
        private readonly ILogger _logger;        

        public RabbitMqLogProvider(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public EasyNetQ.Logging.Logger GetLogger(string name) => LogMessage;

        private bool LogMessage(EasyNetQ.Logging.LogLevel logLevel, Func<string> messageFunc, 
            Exception exception = null, 
            params object[] formatParameters)
        {
            var msg = messageFunc?.Invoke();
            
            if (exception != null)
            {   
                _logger.LogError(exception, msg ?? "EasyNetQ Exception", formatParameters);
                return true;
            }

            if (msg != null)
            {
                _logger.Log((LogLevel)logLevel, msg, formatParameters);
            }
            return true;
        }

        public IDisposable OpenMappedContext(string key, object value,
            bool destructure = false)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }
    }
}