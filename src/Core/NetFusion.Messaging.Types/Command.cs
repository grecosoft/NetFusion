using NetFusion.Base.Entity;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Default implementation representing message that can have one and only one consumer.
    /// The handling consumer can associate a result after processing the message.
    /// </summary>
    public abstract class Command : ICommand
    {
        private IEntityAttributes _attributes;

        public Command()
        {
            _attributes = new EntityAttributes();
        }

        /// <summary>
        /// The optional result of executing the command.
        /// </summary>
        public object Result { get; set; }

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

        public virtual void SetResult(object result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Default implementation representing a message that can have one and only one consumer.  
    /// The handling consumer can associate a result after processing the message.
    /// </summary>
    /// <typeparam name="TResult">The response type of the command.</typeparam>
    public abstract class Command<TResult> : Command, ICommand<TResult>
    {
   
        Type ICommand.ResultType => typeof(TResult);

        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public new TResult Result => (TResult)base.Result;

        public override void SetResult(object result)
        {
            base.SetResult(result);
        }
    }
}
