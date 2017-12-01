using Autofac;
using DomainTests.Queries.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Domain.Patterns.Queries;
using NetFusion.Domain.Patterns.Queries.Config;
using NetFusion.Domain.Patterns.Queries.Dispatch;
using NetFusion.Test.Container;
using System;
using Xunit;

namespace DomainTests.Queries
{
    public class DispatchTests
    {
        [Fact(DisplayName = "Queries: Configured Pre-Filters Unique")]
        public void Configured_PreFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddPreQueryFilter<QueryFilterOne>();
            config.AddPreQueryFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddPreQueryFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
           
        }

        [Fact(DisplayName = "Queries: Configured Post-Filters Unique")]
        public void Configured_PostFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddPostQueryFilter<QueryFilterOne>();
            config.AddPostQueryFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddPostQueryFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
        }

        [Fact(DisplayName = "Queries: Query Cannot have Multiple Consumers")]
        public void Query_CannotHave_MultipleConsumers()
        {
            var typesUnderTest = new[] { typeof(DuplicateConsumerOne), typeof(DuplicateConsumerTwo) };
            var testQuery = new TestQuery();

             ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                        return dispatcher.Dispatch(testQuery);
                    })
                .Result.Assert.Exception<ContainerException>(ex =>
                {
                    ex?.InnerException.Should().NotBeNull();
                    ex.InnerException.Message.Should()
                         .Contain("The following query types have multiple consumers");
                });
            });
        }

        [Fact(DisplayName = "Queries:  Query Must have Consumer")]
        public void Query_MustHave_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQueryNoConsumer();

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                        return dispatcher.Dispatch(testQuery);
                    })
                .Result.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Message.Should().Contain("is not registered");
                });
            });
        }

        [Fact (DisplayName = "Queries: Consumer Can Dispatch Query to Consumer")]
        public void Consumer_Can_DispatchQuery_To_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQuery();

            ContainerFixture.Test(fixture => { fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => 
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                        return dispatcher.Dispatch(testQuery);
                    })
                .Result.Assert.Container(_ =>
                {
                    testQuery.Result.Should().NotBeNull();
                    testQuery.TestLog.Should().HaveCount(1);
                    testQuery.TestLog.Should().Contain(nameof(TestConsumer));
                });
            });
        }

        [Fact(DisplayName = "Queries: Filters Applied Correct Order")]
        public void Filters_Applied_CorrectOrder()
        {
            var typesUnderTest = new[] { typeof(TestConsumer)};
            var testQuery = new TestQuery();
            var dispatchConfig = new QueryDispatchConfig();

            dispatchConfig.AddPreQueryFilter<QueryFilterOne>();
            dispatchConfig.AddPostQueryFilter<QueryFilterTwo>();


            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                    .Container(c => c.WithConfig(dispatchConfig))
                .Act.OnContainer(c => 
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                        return dispatcher.Dispatch(testQuery);
                    })
                .Result.Assert.Container(_ =>
                {
                    testQuery.Result.Should().NotBeNull();
                    testQuery.TestLog.Should().HaveCount(3);
                    testQuery.TestLog[0].Should().Be(nameof(QueryFilterOne));
                    testQuery.TestLog[1].Should().Be(nameof(TestConsumer));
                    testQuery.TestLog[2].Should().Be(nameof(QueryFilterTwo));
                });
            });
        }
    }
}
