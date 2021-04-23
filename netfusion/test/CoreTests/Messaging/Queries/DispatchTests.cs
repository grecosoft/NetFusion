using System;
using System.Threading.Tasks;
using CoreTests.Messaging.Queries.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Test.Container;
using Xunit;

// ReSharper disable All

namespace CoreTests.Messaging.Queries
{
    public class DispatchTests
    {
        [Fact]
        public void Configured_PreFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddFilter<QueryFilterOne>();
            config.AddFilter<QueryFilterTwo>();
            
            Assert.Throws<InvalidOperationException>(() => config.AddFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
           
        }

        [Fact]
        public void Query_CannotHave_MultipleConsumers()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange.Container(c => c.AddMessagingHost().WithMultipleQueryConsumers())
                .Act.RecordException().ComposeContainer()
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.InnerException.Should().NotBeNull();
                    ex.InnerException.Message.Should().Contain("query types have multiple consumers");
                });
            });
        }

        [Fact]
        public Task Query_MustHave_Consumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var query = new MockQuery();
                        await s.GetRequiredService<IMessagingService>()
                            .DispatchAsync(query);
                    });

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.Message.Contains("is not registered");
                });
            });
        }
        
        [Fact]
        public Task Consumer_Can_DispatchQuery_To_Consumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithQueryConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var query = new MockQuery();
                        await s.GetRequiredService<IMessagingService>()
                            .DispatchAsync(query);
                    });

                testResult.Assert.Service<MockQueryConsumer>(qc =>
                {
                    qc.ReceivedQueries.Should().HaveCount(1);
                    qc.ExecutedHandlers.Should().HaveCount(1).And.Contain("Execute");
                });
            });
        }
    }
}
