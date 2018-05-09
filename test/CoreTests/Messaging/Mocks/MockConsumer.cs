using System.Collections.Generic;

namespace CoreTests.Messaging.Mocks
{
    /// <summary>
    /// Message consumer that records the called message
    /// handler methods.
    /// </summary>
    public abstract class MockConsumer
    {
        private readonly IList<string> _executedHandlers = new List<string>();

        public IEnumerable<string> ExecutedHandlers => _executedHandlers;

        protected void AddCalledHandler(string handlerName)
        {
            _executedHandlers.Add(handlerName);
        }
    }
}
