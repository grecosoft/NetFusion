using System;

namespace NetFusion.Messaging.Types.Contracts
{
    /// <summary>
    /// Interface explicitly implemented by the command derived class.
    /// These methods are used internally to access and set the result
    /// of the command. 
    /// </summary>
    public interface ICommandResultState
    {
        /// <summary>
        /// The optional result of the command.
        /// </summary>
        object Result { get; set; }

        /// <summary>
        /// The type of the result associated with the command.
        /// </summary>
        Type ResultType { get; set; }
         
        /// <summary>
        /// Sets the optional result associated with the message.
        /// </summary>
        /// <param name="result">Associated result.</param>
        void SetResult(object result);
    }
}