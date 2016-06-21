using System;
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Default implementation representing message that can have one and only
    /// one consumer and handler.  The handling consumer can associate a 
    /// result after processing the message.
    /// </summary>
    public abstract class Command : ICommand
    {
        private IDictionary<string, object> _attributes;
        public object Result { get; set; }

        public Type ResultType => null;

        void ICommand.SetResult(object result)
        {
            this.Result = result;
        }

        public IDictionary<string, object> Attributes
        {
            get
            {
                return _attributes ?? (_attributes = new Dictionary<string, object>());
            }
        }
    }

    /// <summary>
    /// Default implementation representing a message that can have one and only one 
    /// consumer and handler.  The handling consumer can associate a result 
    /// after processing the message.
    /// </summary>
    /// <typeparam name="TResult">The response set by the consumer that processed
    /// the message.</typeparam>
    public abstract class Command<TResult> : ICommand<TResult>
    {
        private IDictionary<string, object> _attributes;
        public TResult Result { get; set; }

        Type ICommand.ResultType => typeof(TResult);

        void ICommand.SetResult(object result)
        {
            this.Result = (TResult)result;
        }

        public IDictionary<string, object> Attributes
        {
            get
            {
                return _attributes ?? (_attributes = new Dictionary<string, object>());
            }
        }
    }
}
