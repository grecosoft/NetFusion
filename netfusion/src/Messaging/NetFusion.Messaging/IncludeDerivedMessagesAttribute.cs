namespace NetFusion.Messaging
{
    /// <summary>
    /// Used to specify that message handler method, declared as a base message type,
    /// should be called when derived messages are published.  This allows a single
    /// handler to be invoked for a several derived message types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class IncludeDerivedMessagesAttribute : Attribute
    {
    }
}