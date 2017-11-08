using Autofac;
using DomainUnitTests;
using FluentAssertions;
using NetFusion.Base.Scripting;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Entities.Registration;
using NetFusion.Domain.Modules;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.Behaviors.Validation;
using NetFusion.Domain.Patterns.UnitOfWork;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using NetFusion.Testing.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DomainTests.UnitOfWork
{
    public class UnitOfWorkTests
    {
        /// <summary>
        /// Multiple application aggregates can integrate with one another by recording
        /// domain events for occurrences happening in the domain that other aggregates
        /// must be made aware so they can apply their responsible domain logic. 
        /// When a aggregate is saved using the AggregateUnitOfWork, all recorded events
        /// defined for the aggregate being saved are published and handled by application
        /// services managing other aggregates.  
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Aggregates Can Integrate Using Domain Events")]
        public Task AggragatesCanIntegrate_Using_DomainEvents()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };

            return TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => { 
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(new TestCommand());
                },
                (IAppContainer c) =>
                {
                    // Obtain reference to the service that is the receiver of
                    // the integration domain-event:
                    var enlistingSrv = c.Services.Resolve<EnlistingService>();
                    enlistingSrv.ReceivedIntegrationEvent.Should().BeTrue();
                }
            );
        }

        /// <summary>
        /// If the Unit of Work saves without any exceptions being thrown, the domain
        /// events recorded for an aggregate are cleared.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Integration Events Cleared on Successful Commit")]
        public Task IntegrationEvents_Cleared_OnSuccessfulCommit()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand();

            return TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c) =>
                {
                    // Verify that the aggregate used by the test no longer
                    // has any recorded integration events.
                    testCommand.Result.IntegrationEvents().Should().BeEmpty();
                }
            );
        }

        /// <summary>
        /// If an exception is thrown when saving the Unit of Work aggregate recorded
        /// domain events are not cleared.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Integration Events Not Cleared on Save with Exception")]
        public Task IntegrationEvents_NotCleared_OnSaveWithException()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand { EnlistingSrvThrowEx = true };

            return TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c, Exception ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                }
            );
        }

        /// <summary>
        /// When the Unit of Work is saved an exception is thrown if the aggregate
        /// does not pass validation.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Aggregate Not Committed for Invalid Aggregate")]
        public async Task Aggregate_NotCommited_ForInvalidAggregate()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand { IsCommittedAggregateInvalid = true };

            await TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c, Exception ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                }
            );
        }

        [Fact (DisplayName = "Unit-of-Work: Aggregate Not Enlisted for Invalid Aggregate")]
        public async Task Aggregate_NotEnlisted_ForInvalidAggregate()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand { IsEnlistedAggregateInvalid = true };

            await TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c, Exception ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                }
            );
        }

        [Fact (DisplayName = "Unit-of-Work: Consumer Called to Commit when Successful.")]
        public Task Consumer_CalledToCommit_WhenSuccessful()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand();

            return TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c) =>
                {
                    testCommand.Result.WasUowCommited.Should().BeTrue();
                }
            );
        }

        /// <summary>
        /// Integration domain events must be unique within the context of
        /// an unit of work.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Unit-of-work Must Have Unique Integration Events")]
        public Task UnitOfWork_MustHave_UniqueIntegrationEvents()
        {
            var typesUnderTest = new Type[] { typeof(CommitService), typeof(EnlistingService), typeof(BehaviorRegistry) };
            var testCommand = new TestCommand { EnlistDuplicateIntegrationEvent = true };

            return TestContainer(typesUnderTest).Test(
                async (IAppContainer c) => {
                    c.Build().Start();

                    // Publish command.
                    var msgSrv = c.Services.Resolve<IMessagingService>();
                    await msgSrv.SendAsync(testCommand);
                },
                (IAppContainer c, Exception ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                }
            );
        }

        /// <summary>
        /// The unit of work can only be committed once.  All other aggregates 
        /// needed to participate within the unit-of-work must be enlisted.
        /// </summary>
        [Fact(DisplayName = "Unit-of-Work: Unit-of-work Can be Committed only Once")]
        public async Task UnitOfWork_CanBe_CommittedOnlyOnce()
        {
            var entityFactory = EntityFactory.WithIntegration;
            var aggregate = entityFactory.Create<SampleAggregateOne>();
            var uow = new AggregateUnitOfWork(new TestLoggerFactory(), MockMessagingService.Mock);

            await uow.CommitAsync(aggregate, () => Task.CompletedTask);
            (await Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(aggregate, () => Task.CompletedTask)))
                .Message.Contains("The unit-of-work has already being committed.  Other aggregates must be enlisted.");
        }

        /// <summary>
        /// A repository or other consumer can query the unit-of-work for an
        /// already listed aggregate instance.
        /// </summary>
        [Fact(DisplayName = "Unit-of-Work: Consumer Can Query for Committed Aggregate")]
        public async Task Consumer_CanQuery_UnitOfWork_ForEnlistedAggregate()
        {
            var entityFactory = EntityFactory.WithIntegration;
            var aggregate = entityFactory.Create<SampleAggregateOne>();
            var uow = new AggregateUnitOfWork(new TestLoggerFactory(), MockMessagingService.Mock);

            await uow.CommitAsync(aggregate, () => Task.CompletedTask);
            var foundAggregate = uow.GetEnlistedAggregate<SampleAggregateOne>(_ => true);

            foundAggregate.Should().NotBeNull();
            aggregate.Should().BeSameAs(foundAggregate);
        }

        //--------------------- TEST CONTAINER CONFIGURATION -----------------------

        public static ContainerTest TestContainer(params Type[] pluginTypes)
        {
            return ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   // Configure Core Plugin with messaging and the 
                   // unit -of-work module.
                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<UnitOfWorkModule>()
                        .AddPluginType<EntityBehaviorModule>()
                        .UseMessagingPlugin();

                   // Add host plugin with the plugin-types to be used
                   // for the unit-test.
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType(pluginTypes);   
                
               }, c =>
               {
                   c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                   {
                       regConfig.Build = builder =>
                       {
                           builder.RegisterType<NullEntityScriptingService>()
                               .As<IEntityScriptingService>()
                               .SingleInstance();
                       };
                   });
               });
        }

        // Aggregate used for testing integration events with other services.
        public class SampleAggregateOne : IAggregate,
            IValidatableType
        {
            public IBehaviorDelegatee Behaviors { get; private set; }
            public bool WasUowCommited { get; set; }
            private TestCommand _testCommand;

            void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
            {
                Behaviors = behaviors;
            }

            public void TakeAction(TestCommand command)
            {
                _testCommand = command;

                var integrationEvt = new TestIntegrationEvent {
                    EnlistingSrvThrowEx = command.EnlistingSrvThrowEx,
                    EnlistDuplicateIntegrationEvent = command.EnlistDuplicateIntegrationEvent,
                    IsEnlistedAggregateInvalid = command.IsEnlistedAggregateInvalid
                    
                };

                this.IntegrationEvent(integrationEvt);
            }

            public void Validate(IObjectValidator validator)
            {
                validator.Verify(!_testCommand.IsCommittedAggregateInvalid, "Invalidate Aggregate");
            }
        }

        public class SampleAggregateTwo : IAggregate,
            IValidatableType
        {
            public IBehaviorDelegatee Behaviors { get; private set; }
            private bool _makeInvalid { get; set; }

            public void MakeInvalid()
            {
                _makeInvalid = true;
            }

            public void Validate(IObjectValidator validator)
            {
                validator.Verify(!_makeInvalid, "Invalidate Aggregate");
            }

            void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
            {
                Behaviors = behaviors;
            }
        }

        // Test application-service representing the service that will 
        // modify and commit an aggregate.
        public class CommitService : IMessageConsumer
        {
            private readonly IAggregateUnitOfWork _uow;
            private readonly IDomainEntityFactory _entityFactory;

            public CommitService(IAggregateUnitOfWork uow, IDomainEntityFactory entityFactory)
            {
                _uow = uow;
                _entityFactory = entityFactory;
            }

            // Creates a new aggregate and invokes a method representing a change
            // in the state of the aggregate.  The change is registers an integration
            // domain-event used to communicate the change to other interested services.
            [InProcessHandler]
            public async Task<SampleAggregateOne> OnCommand(TestCommand command)
            {
                SampleAggregateOne aggregate = _entityFactory.Create<SampleAggregateOne>();
                aggregate.TakeAction(command);

                await _uow.CommitAsync(aggregate, () => {
                    aggregate.WasUowCommited = true;
                    return Task.CompletedTask;
                });

                return aggregate;
            }
        }

        // Example of another application service consuming an integration event.
        public class EnlistingService : IMessageConsumer
        {
            private readonly IAggregateUnitOfWork _uow;
            private readonly IDomainEntityFactory _entityFactory;
            public bool ReceivedIntegrationEvent { get; private set; } = false;

            public EnlistingService(IAggregateUnitOfWork uow, IDomainEntityFactory entityFactory)
            {
                _uow = uow;
                _entityFactory = entityFactory;
            }

            [InProcessHandler]
            public Task When (TestIntegrationEvent evt)
            {
                ReceivedIntegrationEvent = true;

                if (evt.EnlistingSrvThrowEx)
                {
                    throw new InvalidOperationException(
                        "Enlisting Service Exception");
                }

                var aggregate = _entityFactory.Create<SampleAggregateTwo>();
                if (evt.EnlistDuplicateIntegrationEvent)
                {
                    aggregate.IntegrationEvent(new TestIntegrationEvent());
                   
                }

                if (evt.IsEnlistedAggregateInvalid)
                {
                    aggregate.MakeInvalid();
                }

                return _uow.EnlistAsync(aggregate);
            }
        }

        // Test command to invoke an action to test integration events.
        public class TestCommand : Command<SampleAggregateOne>
        {
            public bool EnlistingSrvThrowEx { get; set; } = false;
            public bool IsCommittedAggregateInvalid { get; set; } = false;
            public bool IsEnlistedAggregateInvalid { get; set; } = false;
            public bool EnlistDuplicateIntegrationEvent { get; set; } = false;
        }

        // Test event recorded by an aggregate used to integrate with
        // other application services.
        public class TestIntegrationEvent : DomainEvent
        {
            public bool EnlistingSrvThrowEx { get; set; } = false;
            public bool EnlistDuplicateIntegrationEvent { get; set; } = false;
            public bool IsEnlistedAggregateInvalid { get; set; } = false;
        }

        public class BehaviorRegistry : IBehaviorRegistry
        {
            public void Register(IFactoryRegistry registry)
            {
                registry.AddBehavior<IValidationBehavior, ValidationBehavior>();
                registry.AddBehavior<IEventIntegrationBehavior, EventIntegrationBehavior>();
            }
        }
    }
}
