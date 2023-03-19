using NetFusion.Common.Base;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Redis.Publisher.Metadata
{
    public abstract class PublishMeta
    {
        public string? ChannelName { get; set; }
        public string ContentType { get; set; } = ContentTypes.Json;

        internal abstract bool Applies(IMessage domainEvent);
        internal abstract string GetEventStateData(IMessage domainEvent);
    }
    
    /// <summary>
    /// Generic derived channel metadata typed to a specific domain-event.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of domain-event.</typeparam>
    public class PublishMeta<TDomainEvent> : PublishMeta
        where TDomainEvent: IDomainEvent
    {
        private Func<TDomainEvent, string> _eventStateData = _ => string.Empty;
        private Func<TDomainEvent, bool> _appliesIf = _ => true;
        
        /// <summary>
        /// Specifies a function that is invoked when a domain-event is being published
        /// to add domain-event specific data to the base channel name.  This allows
        /// subscribers to subscribe to channels based on a specific subset of events.
        /// </summary>
        /// <param name="state">Function passed a domain-event that returns a
        /// string containing event encoded data.  Example: VW.GTI.2017 and if the
        /// channel name were AutoSales, the event would be published to channel:
        /// AutoSales.VW.GTI.2017 and a subscriber could subscribe to:
        /// AutoSales.VW.*.2017</param>
        /// <returns>Self reference.</returns>
        public PublishMeta<TDomainEvent> SetEventData(Func<TDomainEvent, string> state)
        {
            _eventStateData = state ?? throw new ArgumentNullException(nameof(state));
            return this;
        }
        
        /// <summary>
        /// Passed a predicate taking the domain-event instance as a parameter and
        /// returns a boolean value indicating if the domain event should be published.
        /// </summary>
        /// <param name="applies">Predicate passing the domain event returning a boolean value.</param>
        /// <returns>Self Reference.</returns>
        public PublishMeta<TDomainEvent> AppliesWhen(Func<TDomainEvent, bool> applies)
        {
            _appliesIf = applies ?? throw new ArgumentNullException(nameof(applies));
            return this;
        }

        internal override bool Applies(IMessage domainEvent) => _appliesIf((TDomainEvent)domainEvent);

        internal override string GetEventStateData(IMessage domainEvent) =>
            _eventStateData((TDomainEvent) domainEvent);
    }
}