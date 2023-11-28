using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.UnitTests.Messaging;
using NetFusion.Messaging.UnitTests.Queries;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

// ReSharper disable All

namespace CoreTests.Messaging.Queries;

public class QueryDispatchModuleTests
{
    /// <summary>
    /// After the plugin bootstraps, all query consumers will have been found and registered.
    /// </summary>
    [Fact]
    public void AllQueryConsumers_Discovered()
    {
        // ContainerFixture.Test(fixture =>
        // {
        //     fixture.Arrange.Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
        //         .Assert.PluginModule((MessageDispatchModule m) =>
        //         {
        //             var dispatchInfo = m.GetMessageDispatchers(typeof(MockQuery));
        //             
        //             Assert.NotNull(dispatchInfo);
        //             Assert.Equal(typeof(MockQuery), dispatchInfo.QueryType);
        //             Assert.Equal(typeof(MockAsyncQueryConsumer), dispatchInfo.ConsumerType);
        //             Assert.Equal(typeof(MockAsyncQueryConsumer).GetMethod("Execute"), dispatchInfo.HandlerMethod);
        //             Assert.True(dispatchInfo.IsAsync);
        //             Assert.True(dispatchInfo.IsAsyncWithResult);
        //         });
        // });
    }

    /// <summary>
    /// When a query is dispatched, the consumer created is associated with the scope of the request.
    /// </summary>
    [Fact]
    public void AllQueryConsumers_RegisteredAsScopedServices()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
                .Assert.ServiceCollection(s =>
                {
                    var consumerService = s.Where(sd =>
                        sd.ServiceType == typeof(MockAsyncQueryConsumer) &&
                        sd.ImplementationType == typeof(MockAsyncQueryConsumer));

                    consumerService.Should().HaveCount(1);
                });
        });
    }

    /// <summary>
    /// When a query is dispatched, an exception is thrown if there is no corresponding consumer.
    /// </summary>
    [Fact]
    public Task QueryMustHave_OnlyOneConsumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost())
                .Act.RecordException().OnServicesAsync(async s =>
                {
                    var query = new MockQuery();
                    await s.GetRequiredService<IMessagingService>()
                        .ExecuteAsync(query);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                ex.InnerException?.Message.Should().Contain("must have one and only one consumer");
            });
        });
    }

    /// <summary>
    /// When the container is composed, the Query Dispatch Module validates that a query
    /// does not have more than one consumer.
    /// </summary>
    [Fact]
    public void QueryCannotHave_MoreThanOneConsumer()
    {
        // ContainerFixture.Test(fixture =>
        // {
        //     fixture.Arrange.Container(c => c.AddMessagingHost().WithMultipleQueryConsumers())
        //         .Act.RecordException().ComposeContainer()
        //         .Assert.Exception((ContainerException ex) =>
        //             {
        //                 ex.InnerException.Should().NotBeNull().And.BeOfType<QueryDispatchException>();
        //                 ex.InnerException.Message.Should().Contain("query types have multiple consumers");
        //             });
        // });
    }
}