using Autofac;
using CoreTests.Queries.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Test.Container;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CoreTests.Queries
{
    public class DispatchTests
    {
        [Fact(DisplayName = "Queries: Configured Filters Unique")]
        public void Configured_PreFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddQueryFilter<QueryFilterOne>();
            config.AddQueryFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddQueryFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
           
        }

        [Fact(DisplayName = "Queries: Query Cannot have Multiple Consumers")]
        public Task Query_CannotHave_MultipleConsumers()
        {
            var typesUnderTest = new[] { typeof(DuplicateConsumerOne), typeof(DuplicateConsumerTwo) };
            var testQuery = new TestQuery();

             return ContainerFixture.TestAsync(async fixture => {

                 var restResult = await fixture
                .Arrange.Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c =>
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IMessagingService>();
                        return dispatcher.DispatchAsync(testQuery);
                    });

                restResult.Assert.Exception<ContainerException>(ex =>
                {
                    ex?.InnerException.Should().NotBeNull();
                    ex.InnerException.Message.Should()
                         .Contain("The following query types have multiple consumers");
                });
            });
        }

        [Fact(DisplayName = "Queries:  Query Must have Consumer")]
        public Task Query_MustHave_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQueryNoConsumer();

            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                .Act.OnContainer(c => {
                    c.Build().Start();

                    var dispatcher = c.Services.Resolve<IMessagingService>();
                    return dispatcher.DispatchAsync(testQuery);
                });

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.Should().NotBeNull();
                    ex.Message.Should().Contain("is not registered");
                });
            });
        }

        [Fact (DisplayName = "Queries: Consumer Can Dispatch Query to Consumer")]
        public Task Consumer_Can_DispatchQuery_To_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQuery();

            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                    .Act.OnContainer(c =>
                        {
                            c.Build().Start();

                            var dispatcher = c.Services.Resolve<IMessagingService>();
                            return dispatcher.DispatchAsync(testQuery);
                        });

                testResult.Assert.Container(_ =>
                {
                    testQuery.Result.Should().NotBeNull();
                    testQuery.TestLog.Should().HaveCount(1);
                    testQuery.TestLog.Should().Contain(nameof(TestConsumer));
                });
            });
        }

        [Fact(DisplayName = "Queries: Filters Applied Correct Order")]
        public Task Filters_Applied_CorrectOrder()
        {
            var typesUnderTest = new[] { typeof(TestConsumer)};
            var testQuery = new TestQuery();
            var dispatchConfig = new QueryDispatchConfig();

            dispatchConfig.AddQueryFilter<QueryFilterOne>();
            dispatchConfig.AddQueryFilter<QueryFilterTwo>();

            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                    .Container(c => c.WithConfig(dispatchConfig))
                .Act.OnContainer(c =>
                    {
                        c.Build().Start();

                        var dispatcher = c.Services.Resolve<IMessagingService>();
                        return dispatcher.DispatchAsync(testQuery);
                    });

                testResult.Assert.Container(_ =>
                {
                    testQuery.Result.Should().NotBeNull();
                    testQuery.TestLog.Should().HaveCount(5);
                    testQuery.TestLog[0].Should().Be("QueryFilterOne-Pre");
                    testQuery.TestLog[1].Should().Be("QueryFilterTwo-Pre");
                    testQuery.TestLog[2].Should().Be(nameof(TestConsumer));
                    testQuery.TestLog[3].Should().Be("QueryFilterOne-Post");
                    testQuery.TestLog[4].Should().Be("QueryFilterTwo-Post");
                });
            });
        }
    }
}
