using System;
using System.Linq;
using System.Threading.Tasks;
using CoreTests.Messaging.Queries.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.Queries
{
    public class FilterTests
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
        public Task RegisteredFilters_AppliedTo_SentQuery()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var query = new MockQuery();
                
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                    .PluginConfig<QueryDispatchConfig>(config =>
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
                    .PluginConfig<QueryDispatchConfig>(config =>
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

                testResult.Assert.Exception<QueryDispatchException>(ex =>
                {
                    ex.ChildExceptions.Should().HaveCount(1);
                    var enricherEx = ex.ChildExceptions.First();

                    enricherEx.Should().BeOfType<QueryFilterException>();
                    enricherEx.InnerException.Should().BeOfType<InvalidOperationException>();
                    enricherEx.InnerException.Message.Should().Be("TestQueryFilterException");
                });
            });
        }
    }
}