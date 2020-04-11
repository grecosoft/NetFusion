using System;
using System.Collections.Generic;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Default implementation representing message that can have one and only one consumer.
    /// The handling consumer can associate a result after processing the message.  A command
    /// expresses an action that is to take place resulting in a change to an application's state.
    /// </summary>
    public abstract class Command : ICommand, ICommandResultState
    {
        protected Command()
        {
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// List of arbitrary key value pairs associated with the message. 
        /// </summary>
        public IDictionary<string, string> Attributes { get; set; }

      
        // Used by this class and the derived generic command class to access
        // the result state of the command.
        protected ICommandResultState ResultState => this;
        
        
        //-- Explicit implementation of ICommandState used to access the command's result state.
        object ICommandResultState.Result { get; set; }
        Type ICommandResultState.ResultType { get; set; }
        
        
        void ICommandResultState.SetResult(object result)
        {
            // The command result can be null.
            if (result == null) return;
            
            if (! result.GetType().CanAssignTo(ResultState.ResultType))
            {
                throw new InvalidOperationException(
                    $"The handler for the command of type: {GetType()} returned a result of type: {result.GetType()} " + 
                    $"and is not assignable to the command's declared result type of: {ResultState.ResultType}.");
            }
            
            ResultState.Result = result;
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
            ResultState.ResultType = typeof(TResult);
            ResultState.Result = default(TResult);
        }

        /// <summary>
        /// The result of executing the command.
        /// </summary>
        public TResult Result => (TResult)ResultState.Result;
    }
}
