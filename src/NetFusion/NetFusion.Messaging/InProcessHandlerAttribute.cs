using System;

namespace NetFusion.Messaging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InProcessHandlerAttribute : Attribute
    {
    }
}
