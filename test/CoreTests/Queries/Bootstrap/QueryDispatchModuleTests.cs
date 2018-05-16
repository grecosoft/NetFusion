using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.Test.Modules;
using Xunit;
using Xunit.Sdk;

namespace CoreTests.Queries.Bootstrap
{
    public class QueryDispatchModuleTests
    {
        [Fact]
        public void AllQueryConsumers_Discovered()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<QueryDispatchModule>(
                typeof(QueryConsumerOne));
          
            // Act:
            module.Initialize();
            var dispatchInfo = module.GetQueryDispatchInfo(typeof(MockQuery));
            
            // Assert:
            Assert.NotNull(dispatchInfo);
            Assert.Equal(typeof(MockQuery), dispatchInfo.QueryType);
            Assert.Equal(typeof(QueryConsumerOne), dispatchInfo.ConsumerType);
            Assert.Equal(typeof(QueryConsumerOne).GetMethod("OnMockQuery"), dispatchInfo.HandlerMethod);
            Assert.True(dispatchInfo.IsAsync);
            Assert.True(dispatchInfo.IsCancellable);
            Assert.True(dispatchInfo.IsAsyncWithResult);
        }

        [Fact]
        public void AllQueryConsumers_RegisteredAsScopedServcies()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<QueryDispatchModule>(
                typeof(QueryConsumerOne));
            
            var services = new ServiceCollection();
          
            // Act:
            module.Initialize();
            module.RegisterServices(services);
            
            // Assert:
            var consumerType = typeof(QueryConsumerOne);
            
            var consumerService = services.Where(sd =>
                sd.ServiceType == consumerType &&
                sd.ImplementationType == consumerType);
            
            Assert.Equal(1, consumerService.Count());
        }

        [Fact]
        public void QueryCanHave_OnlyOneConsumer()
        {
         
            // Arrange:
            var module = ModuleTestFixture.SetupModule<QueryDispatchModule>(
                typeof(QueryConsumerOne), typeof(QueryConsumerTwo));

            try
            {
                // Act:
                module.Initialize();
            }
            catch (QueryDispatchException ex)
            {
                Assert.True(ex.Message.Contains("A query can only have one consumer"));
                return;
            }

            Assert.True(false, "The expected exception was not raised.");
        }

        public class MockQuery : Query<int>
        {
            
        }

        public class QueryConsumerOne : IQueryConsumer
        {
            public Task<int> OnMockQuery(MockQuery query, CancellationToken cancellationToken)
            {
                Assert.NotNull(query);
                Assert.NotNull(cancellationToken);
                return Task.FromResult(100);
            }
        }
        
        public class QueryConsumerTwo : IQueryConsumer
        {
            public Task<int> OnMockQuery(MockQuery query, CancellationToken cancellationToken)
            {
                Assert.NotNull(query);
                Assert.NotNull(cancellationToken);
                return Task.FromResult(100);
            }
        }
    }
}