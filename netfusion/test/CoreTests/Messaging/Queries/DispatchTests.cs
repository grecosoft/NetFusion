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
        
        [Fact]
        public Task Consumer_Can_DispatchQuery_To_AsyncConsumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var query = new MockQuery();
                        await s.GetRequiredService<IMessagingService>()
                            .DispatchAsync(query);
                    });

                testResult.Assert.Service<MockAsyncQueryConsumer>(qc =>
                {
                    qc.ReceivedQueries.Should().HaveCount(1);
                    qc.ExecutedHandlers.Should().HaveCount(1).And.Contain("Execute");
                });
            });
        }
        
        /// <summary>
        /// Tests that a query handler handler resulting in an exception
        /// is correctly captured.
        /// </summary>
        [Fact]
        public Task ExceptionsCapture_ForDispatched_Query()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var query = new MockQuery {ThrowEx = true};

                        await messagingSrv.DispatchAsync(query);
                    });

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    // Assert that the inner exception is a dispatch exception:
                    ex.InnerException.Should().NotBeNull();
                    // ex.InnerException?.InnerException.Should().BeOfType<MessageDispatchException>();
                });
            });
        }
    }
}
