using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.UnitTests.Queries.Mocks;
using Xunit;

namespace NetFusion.Messaging.UnitTests.Queries;

public class DispatchTests
{
    /// <summary>
    /// A query can be dispatched and handled by a synchronous consumer method handler.
    /// To the calling code, it appears as those it was asynchronous. 
    /// </summary>
    [Fact]
    public Task Consumer_Can_DispatchQuery_To_SyncConsumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                .Act.OnServicesAsync(async s =>
                {
                    var query = new MockQuery();
                    await s.GetRequiredService<IMessagingService>()
                        .DispatchAsync(query);
                });

            testResult.Assert.Service<IMockTestLog>(log =>
            {
                log.Entries.Should().HaveCount(1);
                log.Entries.Should().HaveCount(1).And.Contain("Sync-Command-Handler");
            });
        });
    }
        
    /// <summary>
    /// A query can be dispatched and handled by an asynchronous consumer method handler.
    /// </summary>
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

            testResult.Assert.Service<IMockTestLog>(log =>
            {
                log.Entries.Should().HaveCount(1);
                log.Entries.Should().HaveCount(1).And.Contain("Async-Command-Handler");
            });
        });
    }
}