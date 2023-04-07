using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.UnitTests;
using NetFusion.Messaging.UnitTests.Queries;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

// ReSharper disable All

namespace NetFusion.Messaging.Tests.Queries;

public class FilterTests
{
    [Fact]
    public void Configured_PreFilters_Unique()
    {
        var config = new MessageDispatchConfig();
        config.AddFilter<QueryFilterOne>();
        config.AddFilter<QueryFilterTwo>();
            
        Assert.Throws<BootstrapException>(() => config.AddFilter<QueryFilterOne>())
            .ExceptionId.Should().Be("filter-already-registered");
    }
        
    [Fact]
    public Task RegisteredFilters_AppliedTo_SentQuery()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var query = new MockQuery();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                .PluginConfig<MessageDispatchConfig>(config =>
                {
                    config.AddFilter<QueryFilterOne>();
                    config.AddFilter<QueryFilterTwo>();
                })
                .Act.OnServicesAsync(async s =>
                {
                    await s.GetRequiredService<IMessagingService>()
                        .DispatchAsync(query);
                });

            testResult.Assert.State(() =>
            {
                query.QueryAsserts.Should().HaveCount(4);
                query.QueryAsserts.Should().BeEquivalentTo(
                    "QueryFilterOne-Pre", 
                    "QueryFilterTwo-Pre",
                    "QueryFilterOne-Post", 
                    "QueryFilterTwo-Post");
            });
        });
    }
        
    [Fact]
    public Task IfFilterException_ErrorsAreCaptured()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var query = new MockQuery();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                .PluginConfig<MessageDispatchConfig>(config =>
                {
                    config.AddFilter<QueryFilterOne>();
                })
                .State(() =>
                {
                    query.ThrowInHandlers.Add(nameof(QueryFilterOne));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                {
                    await s.GetRequiredService<IMessagingService>()
                        .DispatchAsync(query);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                ex.ChildExceptions.Should().HaveCount(1);
                var enricherEx = ex.ChildExceptions.First();

                enricherEx.Should().BeOfType<FilterException>();
                enricherEx.InnerException.Should().BeOfType<InvalidOperationException>();
                enricherEx.InnerException.Message.Should().Be("TestQueryFilterException");
            });
        });
    }
}