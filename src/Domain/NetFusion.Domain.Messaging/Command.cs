using NetFusion.Base.Entity;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Messaging
{
    /// <summary>
    /// Default implementation representing message that can have one and only
    /// one message handler.  The handling consumer can associate a result after
    /// processing the message.
    /// </summary>
    public abstract class Command : ICommand
    {
        private IEntityAttributes _attributes;

        /// <summary>
        /// The optional result of executing the command.
        /// </summary>
        public object Result { get; set; }

        public Command()
        {
            _attributes = new EntityAttributes();
        }

        /// <summary>
        /// Dynamic message attributes that can be associated with the command.
        /// </summary>
        public IEntityAttributes Attributes => _attributes;

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }

        Type ICommand.ResultType => null;

        void ICommand.SetResult(object result)
        {
            this.Result = result;
        }
    }

    /// <summary>
    /// Default implementation representing a message that can have one and only one 
    /// consumer handler.  The handling consumer can associate a result after processing 
    /// the message.
    /// </summary>
    /// <typeparam name="TResult">The response type of the command.</typeparam>
    public abstract class Command<TResult> : ICommand<TResult>
    {
        private IEntityAttributes _attributes;

        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public TResult Result { get; set; }

        public Command()
        {
            _attributes = new EntityAttributes();
        }

        /// <summary>
        /// Dynamic message attributes.
        /// </summary>
        public IEntityAttributes Attributes => _attributes;

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }

        Type ICommand.ResultType => typeof(TResult);

        void ICommand.SetResult(object result)
        {
            this.Result = (TResult)result;
        }
    }
}
