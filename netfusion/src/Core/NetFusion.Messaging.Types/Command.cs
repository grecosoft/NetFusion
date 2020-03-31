using NetFusion.Base.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Default implementation representing message that can have one and only one consumer.
    /// The handling consumer can associate a result after processing the message.  A command
    /// expresses an action that is to take place resulting in a change to an application's state.
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
        [JsonIgnore]
        public IEntityAttributes Attributes { get; }

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }

        public Type ResultType { get; protected set; }

        public virtual void SetResult(object result)
        {
            // The command result can be null.
            if (result == null) return;
            
            if (! result.GetType().CanAssignTo(ResultType))
            {
                throw new InvalidOperationException(
                    $"The handler for the command of type: {GetType()} returned a result of type: {result.GetType()} " + 
                    $"and is not assignable to the command's declared result type of: {ResultType}.");
            }
            
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
            ResultType = typeof(TResult);
            base.Result = default(TResult);
        }

        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public new TResult Result => (TResult)base.Result;
    }
}
