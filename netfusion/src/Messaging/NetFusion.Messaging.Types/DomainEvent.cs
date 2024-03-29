﻿using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types;

/// <summary>
/// Base domain-event that can be published to notify application consumers of an occurrence.
/// Domain-events are used to notify one or more consumers of an occurrence of an event.
/// Often, a command handler will publish domain-events used to notify interested subscribers
/// of state changes made by the handling of the command.
/// </summary>
public abstract class DomainEvent : Message, IDomainEvent;