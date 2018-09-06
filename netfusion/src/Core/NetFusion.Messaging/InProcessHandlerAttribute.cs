using System;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Used to specify that a consumer's event handler method should be
    /// invoked by the InProcessMessagePublisher.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InProcessHandlerAttribute : Attribute
    {
    }
}
