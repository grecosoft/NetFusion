using System;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Generic derived channel metadata typed to a specific domain-event.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of domain-event.</typeparam>
    public class ChannelMeta<TDomainEvent> : ChannelMeta
        where TDomainEvent: IDomainEvent
    {
        private Func<TDomainEvent, string> _eventStateData;
    
        private Func<TDomainEvent, bool> _appliesIf = m => true;
        
        public ChannelMeta(string database, string channel) 
            : base(typeof(TDomainEvent), database, channel)
        {
        }

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
        public ChannelMeta<TDomainEvent> SetEventData(Func<TDomainEvent, string> state)
        {
            _eventStateData = state ?? throw new ArgumentNullException(nameof(state));
            return this;
        }
        
        /// <summary>
        /// Passed a predicate taking the domain-event instance as a parameter and
        /// returns a boolean value indicating if the domain event should be published.
        /// </summary>
        /// <param name="applies">Predicate passing the domain event returning
        /// a boolean value.</param>
        /// <returns>Self Reference.</returns>
        public ChannelMeta<TDomainEvent> AppliesWhen(Func<TDomainEvent, bool> applies)
        {
            _appliesIf = applies ?? throw new ArgumentNullException(nameof(applies));
            return this;
        }

        /// <summary>
        /// Specifies the content-type to which the published domain-event should
        /// be serialized.
        /// </summary>
        /// <param name="contentType">The name of the content-type.</param>
        /// <returns>Self reference.</returns>
        public ChannelMeta<TDomainEvent> UseContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type not specified.", nameof(contentType));

            ContentType = contentType;
            
            return this;
        }

        internal override bool Applies(IDomainEvent domainEvent)
        {
            return _appliesIf((TDomainEvent)domainEvent);
        }

        internal override string GetEventStateData(IDomainEvent domainEvent)
        {
            return _eventStateData == null ? string.Empty
                : _eventStateData((TDomainEvent) domainEvent);
        }
    }
}