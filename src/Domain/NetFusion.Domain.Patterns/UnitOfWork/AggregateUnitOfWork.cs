using NetFusion.Common;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.Behaviors.State;
using NetFusion.Domain.Patterns.Behaviors.Validation;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// Saves an updated aggregate and notifies other internal and external aggregates of any changes.
    /// </summary>
    public class AggregateUnitOfWork : IAggregateUnitOfWork
    {
        private readonly IMessagingService _messagingSrv;
        private readonly List<IAggregate> _enlistedAggregates;

        public AggregateUnitOfWork(IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
            _enlistedAggregates = new List<IAggregate>();
        }

        // All the integration events for all enlisted aggregates.
        private IEnumerable<Type> EnlistedIntegrationEventTypes => _enlistedAggregates
            .Select(ea => ea.Behaviors.Get<IEventIntegrationBehavior>().instance)
            .Where(ib => ib != null)
            .SelectMany(ib => ib.DomainEvents.Select(de => de.GetType()));


        public async Task CommitAsync(IAggregate aggregate, Func<Task> commitAction,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(aggregate, nameof(aggregate));
            Check.NotNull(commitAction, nameof(commitAction));

            if (_enlistedAggregates.Any())
            {
                throw new InvalidOperationException(
                    "The unit-of-work has already been commented.  Other aggregates must be enlisted.");
            }

            AssureValidAggregate(aggregate);

            _enlistedAggregates.Add(aggregate);

            // Notify other application aggregates within the same micro-service
            // then commit all changes that occurred;
            await DoInternalIntegration(aggregate, cancellationToken);
            await commitAction();

            // Next, publish all integration events used to notify external 
            // application micro-service aggregates.
            await DoExternalIntegration(cancellationToken);

            // Clear any aggregate recorded integration events and it recorded state.
            FinalizeAggregate();
        }

        public Task EnlistAsync(IAggregate aggregate, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(aggregate, nameof(aggregate));

            if (!_enlistedAggregates.Any())
            {
                throw new InvalidOperationException(
                    "Aggregate can be enlisted only if an aggregate has first been committed.");
            }

            AssureValidAggregate(aggregate);
            AssureNotEnlistedIntegrationEvents(aggregate);
           
            if (!_enlistedAggregates.Contains(aggregate))
            {
                _enlistedAggregates.Add(aggregate);
            }
            return DoInternalIntegration(aggregate, cancellationToken);
        }

        public TAggregate GetEnlistedAggregate<TAggregate>(Func<TAggregate, bool> predicate)
            where TAggregate : IAggregate
        {
            return _enlistedAggregates.OfType<TAggregate>().FirstOrDefault(predicate);
        }

        private void AssureValidAggregate(IAggregate aggregate)
        {
            if (aggregate.Behaviors == null)
            {
                throw new InvalidOperationException(
                    $"The aggregate of type: {aggregate.GetType().FullName} being enlisted, " +
                    $"does not have associated behaviors.");
            }
  
            var behavior = aggregate.Behaviors.Get<IValidationBehavior>();
            behavior.instance?.Validate().ThrowIfInvalid();
        }

        // Integration events for all enlisted aggregates should be unique.  This might not
        // need to be a requirement, but it seems it is best to keep it simple.
        private void AssureNotEnlistedIntegrationEvents(IAggregate aggregate)
        {
            var behavior = aggregate.Behaviors.Get<IEventIntegrationBehavior>();
            if (!behavior.supported)
            {
                return;
            }

            foreach (IDomainEvent domainEvent in behavior.instance.DomainEvents)
            {
                Type domainEventType = domainEvent.GetType();
                if (EnlistedIntegrationEventTypes.Contains(domainEventType))
                {
                    throw new InvalidOperationException(
                        $"The domain event of type: {domainEventType.FullName} is already enlisted.");
                }
            }
        }

        // The following will publish domain events using any publisher within the 'internal' category.
        protected async Task DoInternalIntegration(IAggregate aggregate, CancellationToken cancellationToken)
        {
            var behavior = aggregate.Behaviors.Get<IEventIntegrationBehavior>();
            if (behavior.supported)
            {
                await _messagingSrv.PublishAsync(behavior.instance, cancellationToken, IntegrationTypes.Internal);
                behavior.instance.MarkInternallyIntegrated();
            }
        }

        // The following will publish domain event using any publisher within the 'external' category.
        private async Task DoExternalIntegration(CancellationToken cancellationToken)
        {
            foreach(IAggregate aggregate in _enlistedAggregates)
            {
                await DoExternalIntegration(aggregate, cancellationToken);
            }
        }

        protected Task DoExternalIntegration(IAggregate aggregate, CancellationToken cancellationToken)
        {
            var behavior = aggregate.Behaviors.Get<IEventIntegrationBehavior>();
            if (behavior.supported)
            {
                return _messagingSrv.PublishAsync(behavior.instance, cancellationToken, IntegrationTypes.External);
            }
            return Task.CompletedTask;
        }

        // After the unit-of-work has been successfully committed, clear any information being tracked
        // by the enlisted aggregates.
        private void FinalizeAggregate()
        {
            foreach(IAggregate enlistedAggregate in _enlistedAggregates)
            {
                enlistedAggregate.Behaviors.Get<IEventIntegrationBehavior>().instance?.Clear();
                enlistedAggregate.Behaviors.Get<IAggregateStateBehavior>().instance?.Clear();
            }
        }
    }
}
