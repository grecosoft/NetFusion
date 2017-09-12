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
        public object Result { get; private set; }

        /// <summary>
        /// The type of the optional result.  Null if command does not have an
        /// associated result.
        /// </summary>
        public Type ResultType { get; private set; }

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

        /// <summary>
        /// Sets the optional result of a command.
        /// </summary>
        /// <param name="result">The command's associated result.</param>
        public void SetResult(object result)
        {
            Result = result;
            ResultType = result.GetType();
        }
    }

    /// <summary>
    /// Default implementation representing a message that can have one and only one 
    /// consumer handler.  The handling consumer can associate a result after processing 
    /// the message.
    /// </summary>
    /// <typeparam name="TResult">The response type of the command.</typeparam>
    public abstract class Command<TResult> : Command, ICommand<TResult>
    {
        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public new TResult Result
        {
            get => (TResult)base.Result;
            set => SetResult(value);
        }
    }
}
