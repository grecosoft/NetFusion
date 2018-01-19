using Microsoft.Extensions.Logging;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Logging;
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
    /// If the aggregate being committed or enlisted has validation errors, the commit process stops
    /// and an exception is thrown or the error validations returned to the caller based the specified 
    /// CommitSettings.  Non-error aggregate validations will not throw an exception or stop the commit
    /// process.  If any enlisted aggregates record warning or information validations, the unit-of-work
    /// is committed and the validations returned to the caller.
    /// </summary>
    public class AggregateUnitOfWork : IAggregateUnitOfWork
    {
        private readonly ILogger<AggregateUnitOfWork> _logger;
        private readonly IMessagingService _messagingSrv;

        private readonly List<EnlistedAggregate> _enlistedAggregates;
        private readonly List<ValidationResultSet> _aggregateValidations;

        public AggregateUnitOfWork(ILoggerFactory loggerFactory, IMessagingService messagingSrv)
        {
            _logger = loggerFactory.CreateLogger<AggregateUnitOfWork>();
            _messagingSrv = messagingSrv;

            _enlistedAggregates = new List<EnlistedAggregate>();
            _aggregateValidations = new List<ValidationResultSet>();
        }

        private bool HasErrorValidations => _aggregateValidations.Any(vr => vr.ValidationType == ValidationTypes.Error);
        private IEnumerable<IAggregate> Aggregates => _enlistedAggregates.Select(ea => ea.Aggregate);

        // All the integration events for all enlisted aggregates.
        private IEnumerable<Type> EnlistedIntegrationEventTypes => Aggregates
            .Select(a => a.Behaviors.Get<IEventIntegrationBehavior>())
            .Where(b => b.supported)
            .SelectMany(b => b.instance.AllDomainEvents.Select(de => de.GetType()));


        public Task<CommitResult> CommitAsync(IAggregate aggregate, Func<Task> commitAction,
            CommitSettings settings = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            if (commitAction == null) throw new ArgumentNullException(nameof(commitAction));

            settings = settings ?? new CommitSettings();

            if (_enlistedAggregates.Any())
            {
                throw new InvalidOperationException(
                    "The unit-of-work is being committed or a prior commit-result has not been disposed.  " + 
                    "Other aggregates must be enlisted or the last commit-result disposed.");
            }

            ValidateAggregate(aggregate);

            _enlistedAggregates.Add(new EnlistedAggregate
            {
                Aggregate = aggregate,
                CommitAction = commitAction
            });

            using (var logger = _logger.LogTraceDuration(UnitOfWorkLogEvents.COMMIT_DETAILS, 
                $"Committing Aggregate of type: {aggregate.GetType().FullName}"))
            {
                logger.Log.LogTraceDetails("Commit Settings", settings);
                logger.Log.LogTrace("Number Enlisted Aggregates: {NumEnlistedAggregates}", _enlistedAggregates.Count());

                return IntegrateAggregate(aggregate, settings, cancellationToken);
            }
        }

        private async Task<CommitResult> IntegrateAggregate(IAggregate aggregate, CommitSettings settings, CancellationToken cancellationToken)
        {
            // Notify other application aggregates within the same micro-service.
            await DoInternalIntegration(aggregate, cancellationToken);

            if (HasErrorValidations)
            {
                if (settings.ThrowIfInvalid)
                {
                    var errorValResult = _aggregateValidations.First(av => av.ValidationType == ValidationTypes.Error);
                    errorValResult.ThrowIfInvalid();
                }
                return CommitResult.Invalid(this, _aggregateValidations);
            }

            // Commit all enlisted aggregates.
            foreach (EnlistedAggregate enlisted in _enlistedAggregates.Where(ea => ea.CommitAction != null))
            {
                await enlisted.CommitAction();
            }

            // Next, publish all integration events used to notify external 
            // application micro-service aggregates.
            await DoExternalIntegration(cancellationToken);

            return CommitResult.Sucessful(this, _aggregateValidations);
        }


        public Task EnlistAsync(IAggregate aggregate,
            Func<Task> commitAction = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            if (!_enlistedAggregates.Any())
            {
                throw new InvalidOperationException(
                    "Aggregate can be enlisted only if an aggregate has first been committed.");
            }

            ValidateAggregate(aggregate);
            AssureNotEnlistedIntegrationEvents(aggregate);
           
            if (!IsEnlisted(aggregate))
            {          
                _enlistedAggregates.Add(new EnlistedAggregate {
                    Aggregate = aggregate,
                    CommitAction = commitAction });
            }

            return DoInternalIntegration(aggregate, cancellationToken);
        }

        public bool IsEnlisted (IAggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));

            return Aggregates.Contains(aggregate);
        }

        public TAggregate GetEnlistedAggregate<TAggregate>(Func<TAggregate, bool> predicate)
            where TAggregate : IAggregate
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Aggregates.OfType<TAggregate>().FirstOrDefault(predicate);
        }

        private void ValidateAggregate(IAggregate aggregate)
        {
            if (aggregate.Behaviors == null)
            {
                throw new InvalidOperationException(
                    $"The aggregate of type: {aggregate.GetType().FullName} being enlisted, " +
                    $"does not have associated behaviors.");
            }
  
            var behavior = aggregate.Behaviors.Get<IValidationBehavior>();

            ValidationResultSet valResult = behavior.instance?.Validate();                     
            if (valResult != null && valResult.ObjectValidations.Any())
            {
                _aggregateValidations.Add(valResult);
            }
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

            foreach (IDomainEvent domainEvent in behavior.instance.AllDomainEvents)
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
            if (behavior.supported && !HasErrorValidations)
            {
                LogIntegrationEvents(IntegrationTypes.Internal, aggregate);

                foreach (IDomainEvent domainEvent in behavior.instance.NonIntegratedEvents)
                {
                    await _messagingSrv.PublishAsync(domainEvent, cancellationToken, IntegrationTypes.Internal);
                }

                behavior.instance.MarkInternallyIntegrated();
            }
        }

        // The following will publish domain event using any publisher within the 'external' category.
        private async Task DoExternalIntegration(CancellationToken cancellationToken)
        {
            foreach(IAggregate aggregate in Aggregates)
            {
                await DoExternalIntegration(aggregate, cancellationToken);
            }
        }

        protected async Task DoExternalIntegration(IAggregate aggregate, CancellationToken cancellationToken)
        {
            var behavior = aggregate.Behaviors.Get<IEventIntegrationBehavior>();
            if (behavior.supported)
            {
                LogIntegrationEvents(IntegrationTypes.External, aggregate);
                foreach (IDomainEvent domainEvent in behavior.instance.AllDomainEvents)
                {
                    await _messagingSrv.PublishAsync(domainEvent, cancellationToken, IntegrationTypes.External);
                }
            }
            await Task.CompletedTask;
        }

        private void LogIntegrationEvents(IntegrationTypes integrationType, IAggregate aggregate)
        {
            var integrationBehavior = aggregate.Behaviors.GetRequired<IEventIntegrationBehavior>();

            var eventTypeNames = integrationBehavior.NonIntegratedEvents
                .Select(de => de.GetType().FullName)
                .ToArray();

            _logger.LogTrace(UnitOfWorkLogEvents.INTEGRATION_DETAILS,
                "Aggregate: {Aggregate Type} Integration Type: {Integration Type} Integration Events {Events}", 
                    aggregate.GetType().FullName, 
                    integrationType, 
                    string.Join(", ", eventTypeNames));
        }

        // After the unit-of-work has been successfully committed, clear any information being tracked
        // by the enlisted aggregates.
        public void Clear()
        {
            foreach(IAggregate enlistedAggregate in Aggregates)
            {
                enlistedAggregate.Behaviors.Get<IEventIntegrationBehavior>().instance?.Clear();
                enlistedAggregate.Behaviors.Get<IAggregateStateBehavior>().instance?.Clear();
            }

            _aggregateValidations.Clear();
            _enlistedAggregates.Clear();
        }

        private class EnlistedAggregate
        {
            public IAggregate Aggregate { get; set; }
            public Func<Task> CommitAction { get; set; }
        }
    }
}
