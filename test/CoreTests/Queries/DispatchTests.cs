using CoreTests.Queries.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Test.Container;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using NetFusion.Bootstrap.Exceptions;

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
        public void Query_CannotHave_MultipleConsumers()
        {
            var typesUnderTest = new[] { typeof(DuplicateConsumerOne), typeof(DuplicateConsumerTwo) };
            var testQuery = new TestQuery();

            ContainerFixture.Test((Action<ContainerFixture>)(fixture =>
            {
                try
                {
                    fixture.Arrange.Resolver((Action<NetFusion.Test.Plugins.TestTypeResolver>)(r => r.WithDispatchConfiguredHost((Type[])typesUnderTest)));
                    fixture.InitContainer();
                    Assert.True(false, "Expected exception not raised.");
                }
                catch (ContainerException ex)
                {
                    ex.InnerException.Should().NotBeNull();
                    ex.InnerException.Message.Should()
                         .Contain("The following query types have multiple consumers");
                }
                catch
                {
                    Assert.True(false, "Unexpected exception raised.");
                }
            }));
        }

        [Fact(DisplayName = "Queries:  Query Must have Consumer")]
        public Task Query_MustHave_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQueryNoConsumer();

            return ContainerFixture.TestAsync((Func<ContainerFixture, Task>)(async fixture =>
            {
                var testResult = await fixture.Arrange
                        .Resolver((Action<NetFusion.Test.Plugins.TestTypeResolver>)(r => r.WithDispatchConfiguredHost((Type[])typesUnderTest)))
                    .Act.OnServices(s =>
                    {

                        var dispatcher = s.GetService<IMessagingService>();
                        return dispatcher.DispatchAsync(testQuery);
                    });

                testResult.Assert.Exception<QueryDispatchException>((QueryDispatchException ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Message.Should().Contain("is not registered");
                });
            }));
        }
        
        [Fact(DisplayName = "Queries: Consumer Can Dispatch Query to Consumer")]
        public Task Consumer_Can_DispatchQuery_To_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQuery();

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                        .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                        {
                           
                            var dispatcher = s.GetService<IMessagingService>();
                            return dispatcher.DispatchAsync(testQuery);
                        });

                testResult.Assert.State(() =>
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
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQuery();
            var dispatchConfig = new QueryDispatchConfig();

            dispatchConfig.AddQueryFilter<QueryFilterOne>();
            dispatchConfig.AddQueryFilter<QueryFilterTwo>();

            return ContainerFixture.TestAsync(async fixture =>
            {

                var testResult = await fixture.Arrange
                        .Resolver(r => r.WithDispatchConfiguredHost(typesUnderTest))
                        .Container(c => c.WithConfig(dispatchConfig))
                    .Act.OnServices(s =>
                    {

                        var dispatcher = s.GetService<IMessagingService>();
                        return dispatcher.DispatchAsync(testQuery);
                    });

                testResult.Assert.State(() =>
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
