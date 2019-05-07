using CoreTests.Queries.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Test.Plugins;

namespace CoreTests.Queries
{
    public class QueryDispatchTests
    {
        [Fact(DisplayName = "Queries: Configured Filters Unique")]
        public void Configured_PreFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddFilter<QueryFilterOne>();
            config.AddFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
           
        }

        [Fact(DisplayName = "Queries: Query Cannot have Multiple Consumers")]
        public void Query_CannotHave_MultipleConsumers()
        {
            ContainerFixture.Test(fixture =>
            {
                try
                {
                    fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<DuplicateConsumerOne>();
                        hostPlugin.AddPluginType<DuplicateConsumerTwo>();
                        
                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                        
                    });
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
            });
        }

        [Fact(DisplayName = "Queries:  Query Must have Consumer")]
        public Task Query_MustHave_Consumer()
        {
            var testQuery = new TestQueryNoConsumer();

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<TestConsumer>();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Act.OnServices(s =>
                    {

                        var dispatcher = s.GetService<IMessagingService>();
                        return dispatcher.DispatchAsync(testQuery);
                    });

                testResult.Assert.Exception((QueryDispatchException ex) =>
                {
                    ex.Should().NotBeNull();
                    ex.Message.Should().Contain("is not registered");
                });
            });
        }
        
        [Fact(DisplayName = "Queries: Consumer Can Dispatch Query to Consumer")]
        public Task Consumer_Can_DispatchQuery_To_Consumer()
        {
            var testQuery = new TestQuery();

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture
                    .Arrange
                        .Container(c =>
                        {
                            var hostPlugin = new MockHostPlugin();
                            hostPlugin.AddPluginType<TestConsumer>();

                            c.RegisterPlugins(hostPlugin);
                            c.RegisterPlugin<MessagingPlugin>();
                        })
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

//        [Fact(DisplayName = "Queries: Filters Applied Correct Order")]
//        public Task Filters_Applied_CorrectOrder()
//        {
//            var typesUnderTest = new[] { typeof(TestConsumer) };
//            var testQuery = new TestQuery();
//            var dispatchConfig = new QueryDispatchConfig();
//
//            dispatchConfig.AddFilter<QueryFilterOne>();
//            dispatchConfig.AddFilter<QueryFilterTwo>();
//
//            return ContainerFixture.TestAsync(async fixture =>
//            {
//
//                var testResult = await fixture.Arrange
//                        .Container(c => c.WithDispatchConfiguredHost(typesUnderTest))
//                        .Container(c => c.WithConfig(dispatchConfig))
//                    .Act.OnServices(s =>
//                    {
//
//                        var dispatcher = s.GetService<IMessagingService>();
//                        return dispatcher.DispatchAsync(testQuery);
//                    });
//
//                testResult.Assert.State(() =>
//                {
//                    testQuery.Result.Should().NotBeNull();
//                    testQuery.TestLog.Should().HaveCount(5);
//                    testQuery.TestLog[0].Should().Be("QueryFilterOne-Pre");
//                    testQuery.TestLog[1].Should().Be("QueryFilterTwo-Pre");
//                    testQuery.TestLog[2].Should().Be(nameof(TestConsumer));
//                    testQuery.TestLog[3].Should().Be("QueryFilterOne-Post");
//                    testQuery.TestLog[4].Should().Be("QueryFilterTwo-Post");
//                });
//            });
//        }
    }
}
