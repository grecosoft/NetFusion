﻿using NetFusion.Base.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Default implementation representing message that can have one and only one consumer.
    /// The handling consumer can associate a result after processing the message.
    /// </summary>
    public abstract class Command : ICommand
    {
        protected Command()
        {
            Attributes = new EntityAttributes();
        }

        /// <summary>
        /// The optional result of executing the command.
        /// </summary>
        public object Result { get; protected set; }

        /// <summary>
        /// Dynamic message attributes that can be associated with the command.
        /// </summary>
        [IgnoreDataMember]
        public IEntityAttributes Attributes { get; }

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }

        Type ICommand.ResultType => null;

        public virtual void SetResult(object result)
        {
            // The result can be null.
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
        protected Command()
        {
            base.Result = default(TResult);
        }

        Type ICommand.ResultType => typeof(TResult);

        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public new TResult Result => (TResult)base.Result;
    }
}
