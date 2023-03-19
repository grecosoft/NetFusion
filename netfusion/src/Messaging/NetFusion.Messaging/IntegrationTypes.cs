namespace NetFusion.Messaging;

/// <summary>
/// Determines the scope to which a given message publisher
/// sends domain events to interested subscribers.
/// </summary>
public enum IntegrationTypes
{
    /// <summary>
    /// Used to specify all integration types apply.
    /// </summary>
    All = 0,

    /// <summary>
    /// The publisher dispatches the event to subscribers located
    /// within the same process as the publisher.
    /// </summary>
    Internal = 1,

    /// <summary>
    /// The publisher dispatches the event to subscribers located
    /// external to the publisher via a central message bus.
    /// </summary>
    External = 2
}