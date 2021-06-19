using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Test
{
    public class ActTests
    {
        /// <summary>
        /// The created composite application can be acted on.  Once of the most
        /// common actions taken is to start the composite application.
        /// </summary>
        [Fact]
        public void CanActOn_CompositeApplication()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    }).Act.OnApplication(app => app.Start())
                    .Assert.CompositeApp(app =>
                    {
                        app.IsStarted.Should().BeTrue();
                    });
            });
        }
        
        /// <summary>
        /// The created composite application can be acted on.  Once of the most
        /// common actions taken is to start the composite application.
        /// </summary>
        [Fact]
        public Task CanActOn_CompositeApplicationAsync()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnApplicationAsync(app => app.StartAsync());

                testResult.Assert.CompositeApp(app =>
                {
                    app.IsStarted.Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// Multiple services registered by plugins can be acted on by obtaining
        /// a reference to the services by acting on the created service provider.
        /// </summary>
        [Fact]
        public void CanActOn_ServiceProvider()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<TestModule>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    }).Act.OnServices(s =>
                    {
                        var testSrv = s.GetRequiredService<ITestService>();
                        testSrv.SetValue("Acted-On-Value");
                    })
                    .Assert.Services(s =>
                    {
                        var testSrv = s.GetRequiredService<ITestService>();
                        testSrv.GetValue().Should().Be("Acted-On-Value");
                    });
            });
        }

        /// <summary>
        /// Multiple services registered by plugins can be acted on by obtaining
        /// a reference to the services by acting on the created service provider.
        /// </summary>
        [Fact]
        public Task CanActOn_ServiceProviderAsync()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange.Container(c =>
                {
                    var corePlugin = new MockCorePlugin();
                    corePlugin.AddModule<TestModule>();

                    c.RegisterPlugin<MockHostPlugin>();
                    c.RegisterPlugins(corePlugin);
                }).Act.OnServicesAsync(async s =>
                {
                    var testSrv = s.GetRequiredService<ITestService>();
                    await testSrv.SetValueAsync(400);
                });

                await testResult.Assert.ServicesAsync(async s =>
                {
                    var testSrv = s.GetRequiredService<ITestService>();
                    var result = await testSrv.GetValueAsync();
                    
                    result.Should().Be("400");
                });

            });
        }

        /// <summary>
        /// A specific service registered by a plugin can be acted on.
        /// </summary>
        [Fact]
        public void CanActOn_Service()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<TestModule>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    }).Act.OnService<ITestService>(testSrv =>
                    {
                        testSrv.SetValue("Acted-On-Value");
                    })
                    .Assert.Service<ITestService>(testSrv =>
                    {
                        testSrv.GetValue().Should().Be("Acted-On-Value");
                    });
            });
        }

        /// <summary>
        /// A specific service registered by a plugin can be acted on.
        /// </summary>
        [Fact]
        public Task CanActOn_ServiceAsync()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange.Container(c =>
                {
                    var corePlugin = new MockCorePlugin();
                    corePlugin.AddModule<TestModule>();

                    c.RegisterPlugin<MockHostPlugin>();
                    c.RegisterPlugins(corePlugin);
                }).Act.OnServiceAsync<ITestService>(async testSrv =>
                {
                    await testSrv.SetValueAsync(200);
                });

                await testResult.Assert.ServiceAsync<ITestService>(async testSrv =>
                {
                    var result = await testSrv.GetValueAsync();
                    result.Should().Be("200");
                });
            });
        }
        
        //-- Types used by unit-tests:
        
        private class TestModule : PluginModule
        {
            public override void RegisterServices(IServiceCollection services)
            {
                services.AddScoped<ITestService, TestService>();
            }
        }
        
        private interface ITestService
        {
            void SetValue(string value);
            Task SetValueAsync(int value);
            string GetValue();
            Task<string> GetValueAsync();
        }

        public class TestService : ITestService
        {
            private string _value;

            public void SetValue(string value) => _value = value;
            public string GetValue() => _value;

            public Task SetValueAsync(int value)
            {
                _value = value.ToString();
                return Task.CompletedTask;
            }

            public Task<string> GetValueAsync() => Task.FromResult(_value);
        }
    }
}