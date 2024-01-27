namespace NetFusion.Messaging.Types.Contracts;

/// <summary>
/// Message that can be published to notify application consumers of an occurrence.
/// Domain-events are used to notify one or more consumers of an occurrence of an event.
/// Often, command handlers will publish domain-events used to notify interested subscribers
/// of state changes made by the handling of the command.
/// </summary>
public interface IDomainEvent : IMessage;