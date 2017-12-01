using Autofac;
using DomainTests.UnitOfWork.Mocks;
using DomainUnitTests;
using FluentAssertions;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.UnitOfWork;
using NetFusion.Messaging;
using NetFusion.Test.Container;
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
        public void AggragatesCanIntegrate_Using_DomainEvents()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(new MockCommand());
                    })
                .Result.Assert.Container(c =>
                {
                    // Obtain reference to the service that is the receiver of
                    // the integration domain-event:
                    var enlistingSrv = c.Services.Resolve<MockEnlistingService>();
                    enlistingSrv.ReceivedIntegrationEvent.Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// If the Unit of Work saves without any exceptions being thrown, the domain
        /// events recorded for an aggregate are cleared.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Integration Events Cleared on Successful Commit")]
        public void IntegrationEvents_Cleared_OnSuccessfulCommit()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand();

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Container(_ =>
                {
                    // Verify that the aggregate used by the test no longer
                    // has any recorded integration events.
                    testCommand.Result.IntegrationEvents().Should().BeEmpty();
                });
            });
        }

        /// <summary>
        /// If an exception is thrown when saving the Unit of Work aggregate recorded
        /// domain events are not cleared.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Integration Events Not Cleared on Save with Exception")]
        public void IntegrationEvents_NotCleared_OnSaveWithException()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand { EnlistingSrvThrowEx = true };

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Exception<PublisherException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                });
            });
        }

        /// <summary>
        /// When the Unit of Work is saved an exception is thrown if the aggregate
        /// does not pass validation.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Aggregate Not Committed for Invalid Aggregate")]
        public void Aggregate_NotCommited_ForInvalidAggregate()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand { IsCommittedAggregateInvalid = true };

             ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Exception<PublisherException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                });
            });
        }

        [Fact (DisplayName = "Unit-of-Work: Aggregate Not Enlisted for Invalid Aggregate")]
        public void Aggregate_NotEnlisted_ForInvalidAggregate()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand { IsEnlistedAggregateInvalid = true };

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Exception<PublisherException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                });
            });
        }

        [Fact (DisplayName = "Unit-of-Work: Consumer Called to Commit when Successful.")]
        public void Consumer_CalledToCommit_WhenSuccessful()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand();

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Container(_ =>
                {
                    testCommand.Result.WasUowCommited.Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// Integration domain events must be unique within the context of
        /// an unit of work.
        /// </summary>
        [Fact (DisplayName = "Unit-of-Work: Unit-of-work Must Have Unique Integration Events")]
        public void UnitOfWork_MustHave_UniqueIntegrationEvents()
        {
            var typesUnderTest = new Type[] { typeof(MockCommitService), typeof(MockEnlistingService), typeof(MockBehaviorRegistry) };
            var testCommand = new MockCommand { EnlistDuplicateIntegrationEvent = true };

             ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.UsingDefaultServices();
                        c.Build().Start();

                        var msgSrv = c.Services.Resolve<IMessagingService>();
                        return msgSrv.SendAsync(testCommand);
                    })
                .Result.Assert.Exception<PublisherException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Should().BeOfType<PublisherException>();
                });
            });
        }

        /// <summary>
        /// The unit of work can only be committed once.  All other aggregates 
        /// needed to participate within the unit-of-work must be enlisted.
        /// </summary>
        [Fact(DisplayName = "Unit-of-Work: Unit-of-work Can be Committed only Once")]
        public async Task UnitOfWork_CanBe_CommittedOnlyOnce()
        {
            var entityFactory = EntityFactory.WithIntegration;
            var aggregate = entityFactory.Create<MockAggregateOne>();
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
            var aggregate = entityFactory.Create<MockAggregateOne>();
            var uow = new AggregateUnitOfWork(new TestLoggerFactory(), MockMessagingService.Mock);

            await uow.CommitAsync(aggregate, () => Task.CompletedTask);
            var foundAggregate = uow.GetEnlistedAggregate<MockAggregateOne>(_ => true);

            foundAggregate.Should().NotBeNull();
            aggregate.Should().BeSameAs(foundAggregate);
        }
    }
}
