using System;
using System.Threading.Tasks;
using CoreTests.Messaging.Queries.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.Queries
{
    public class DispatchExceptionTests
    {
        /// <summary>
        /// An exception is raised if a query is dispatched and there is no
        /// corresponding query consumer.
        /// </summary>
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
                    ex.Message.Should().Contain("is not registered");
                });
            });
        }
                
        /// <summary>
        /// Validates that an exception thrown by a asynchronous command handler is correctly captured. 
        /// </summary>
        [Fact]
        public Task ExceptionsCapture_ForDispatched_AsyncConsumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var query = new MockQuery();
                
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
                    .State(() =>
                    {
                        query.ThrowInHandlers.Add(nameof(MockAsyncQueryConsumer));
                    })
                    .Act.RecordException().OnServicesAsync(async s =>
                    { 
                        await s.GetRequiredService<IMessagingService>()
                            .DispatchAsync(query);
                    });

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.InnerException.Should().BeOfType<InvalidOperationException>();
                    ex.InnerException.Message.Should().Be($"{nameof(MockAsyncQueryConsumer)}_Exception");
                });
            });
        }

        /// <summary>
        /// Validates that an exception thrown by a synchronous command handler is correctly captured. 
        /// </summary>
        [Fact]
        public Task ExceptionsCapture_ForDispatched_SyncConsumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var query = new MockQuery();
                
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                    .State(() =>
                    {
                        query.ThrowInHandlers.Add(nameof(MockSyncQueryConsumer));
                    })
                    .Act.RecordException().OnServicesAsync(async s =>
                    { 
                        await s.GetRequiredService<IMessagingService>()
                            .DispatchAsync(query);
                    });

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.InnerException.Should().BeOfType<InvalidOperationException>();
                    ex.InnerException.Message.Should().Be($"{nameof(MockSyncQueryConsumer)}_Exception");
                });
            });
        }
    }
}