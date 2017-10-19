using Autofac;
using DomainTests.Queries.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Patterns.Queries;
using NetFusion.Domain.Patterns.Queries.Config;
using NetFusion.Domain.Patterns.Queries.Filters;
using NetFusion.Domain.Patterns.Queries.Modules;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DomainTests.Queries
{
    public class DispatchTests
    {
        [Fact(DisplayName = "Queries: Configured Pre-Filters Unique")]
        public void Configured_PreFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddPreQueryFilter<QueryFilterOne>();
            config.AddPreQueryFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddPreQueryFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
           
        }

        [Fact(DisplayName = "Queries: Configured Post-Filters Unique")]
        public void Configured_PostFilters_Unique()
        {
            var config = new QueryDispatchConfig();
            config.AddPostQueryFilter<QueryFilterOne>();
            config.AddPostQueryFilter<QueryFilterTwo>();

            Assert.Throws<InvalidOperationException>(() => config.AddPostQueryFilter<QueryFilterOne>())
                .Message.Should().Contain("has already been added");
        }

        [Fact(DisplayName = "Queries: Query Cannot have Multiple Consumers")]
        public Task Query_CannotHave_MultipleConsumers()
        {
            var typesUnderTest = new[] { typeof(DuplicateConsumerOne), typeof(DuplicateConsumerTwo) };
            var testQuery = new TestQuery();

            return TestContainer(typesUnderTest).Test(
               async (IAppContainer c) => {
                   c.Build().Start();

                   // Publish command.
                   var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                   await dispatcher.Dispatch(testQuery);
               },
               (IAppContainer c, Exception ex) =>
               {
                   ex?.InnerException.Should().NotBeNull();
                   ex.InnerException.Message.Should()
                        .Contain("The following query types have multiple consumers");
               }
           );
        }

        [Fact(DisplayName = "Queries:  Query Must have Consumer")]
        public Task Query_MustHave_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQueryNoConsumer();

            return TestContainer(typesUnderTest).Test(
               async (IAppContainer c) => {
                   c.Build().Start();

                   // Publish command.
                   var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                   await dispatcher.Dispatch(testQuery);
               },
               (IAppContainer c, Exception ex) =>
               {
                   ex.Should().NotBeNull();
                   ex.Message.Should().Contain("is not registered");
               }
           );
        }

        [Fact (DisplayName = "Queries: Consumer Can Dispatch Query to Consumer")]
        public Task Consumer_Can_DispatchQuery_To_Consumer()
        {
            var typesUnderTest = new[] { typeof(TestConsumer) };
            var testQuery = new TestQuery();

            return TestContainer(typesUnderTest).Test(
               async (IAppContainer c) => {
                   c.Build().Start();

                    // Publish command.
                    var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                   await dispatcher.Dispatch(testQuery);
               },
               (IAppContainer c) =>
               {
                   testQuery.Result.Should().NotBeNull();
                   testQuery.TestLog.Should().HaveCount(1);
                   testQuery.TestLog.Should().Contain(nameof(TestConsumer));
               }
           );
        }

        [Fact(DisplayName = "Queries: Filters Applied Correct Order")]
        public Task Filters_Applied_CorrectOrder()
        {
            var typesUnderTest = new[] { typeof(TestConsumer)};
            var testQuery = new TestQuery();
            var dispatchConfig = new QueryDispatchConfig();

            dispatchConfig.AddPreQueryFilter<QueryFilterOne>();
            dispatchConfig.AddPostQueryFilter<QueryFilterTwo>();

            return TestContainer(typesUnderTest).Test(
               async (IAppContainer c) => {
                   c.WithConfig(dispatchConfig);
                   c.Build().Start();

                   // Publish command.
                   var dispatcher = c.Services.Resolve<IQueryDispatcher>();
                   await dispatcher.Dispatch(testQuery);
               },
               (IAppContainer c) =>
               {
                    testQuery.Result.Should().NotBeNull();
                    testQuery.TestLog.Should().HaveCount(3);
                    testQuery.TestLog[0].Should().Be(nameof(QueryFilterOne));
                    testQuery.TestLog[1].Should().Be(nameof(TestConsumer));
                    testQuery.TestLog[2].Should().Be(nameof(QueryFilterTwo));
               }
           );
        }

        //--------------------- TEST CONTAINER CONFIGURATION -----------------------

        public static ContainerTest TestContainer(params Type[] pluginTypes)
        {
            return ContainerSetup
               .Arrange((TestTypeResolver config) =>
               {
                   // Configure Core Plugin with messaging and the 
                   // unit -of-work module.
                   config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<QueryDispatchModule>()
                        .AddPluginType<QueryFilterModule>()
                        .AddPluginType<QueryDispatchConfig>();

                   // Add host plugin with the plugin-types to be used
                   // for the unit-test.
                   config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType(pluginTypes);

               }, c =>
               {
                   c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                   {
                       regConfig.Build = builder =>
                       {
                          
                       };
                   });
               });
        }
    }
}
