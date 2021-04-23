using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Messaging.Types;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Messaging.Queries
{
    public class QueryDispatchModuleTests
    {
        [Fact]
        public void AllQueryConsumers_Discovered()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<QueryConsumerOne>();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.PluginModule((QueryDispatchModule m) =>
                    {
                        var dispatchInfo = m.GetQueryDispatchInfo(typeof(MockQuery));
                        
                        Assert.NotNull(dispatchInfo);
                        Assert.Equal(typeof(MockQuery), dispatchInfo.QueryType);
                        Assert.Equal(typeof(QueryConsumerOne), dispatchInfo.ConsumerType);
                        Assert.Equal(typeof(QueryConsumerOne).GetMethod("OnMockQuery"), dispatchInfo.HandlerMethod);
                        Assert.True(dispatchInfo.IsAsync);
                        Assert.True(dispatchInfo.IsCancellable);
                        Assert.True(dispatchInfo.IsAsyncWithResult);
                    });
            });
        }

        [Fact]
        public void AllQueryConsumers_RegisteredAsScopedServices()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<QueryConsumerOne>();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Assert.ServiceCollection(s =>
                    {
                        var consumerService = s.Where(sd =>
                            sd.ServiceType == typeof(QueryConsumerOne) &&
                            sd.ImplementationType == typeof(QueryConsumerOne));

                        consumerService.Should().HaveCount(1);
                    });
            });
        }

        [Fact]
        public void QueryCanHave_OnlyOneConsumer()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<QueryConsumerOne>();
                        hostPlugin.AddPluginType<QueryConsumerTwo>();

                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugin<MessagingPlugin>();
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception((ContainerException ex) =>
                        {
                            ex.InnerException.Should().BeOfType<QueryDispatchException>();
                        });
            });
        }

        public class MockQuery : Query<int>
        {
            
        }

        public class QueryConsumerOne : IQueryConsumer
        {
            [InProcessHandler]
            public Task<int> OnMockQuery(MockQuery query, CancellationToken cancellationToken)
            {
                Assert.NotNull(query);
                return Task.FromResult(100);
            }
        }
        
        public class QueryConsumerTwo : IQueryConsumer
        {
            [InProcessHandler]
            public Task<int> OnMockQuery(MockQuery query, CancellationToken cancellationToken)
            {
                Assert.NotNull(query);
                return Task.FromResult(100);
            }
        }
    }
}