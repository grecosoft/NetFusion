using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Test
{
    /// <summary>
    /// Additional assert methods not used within ActTests.
    /// </summary>
    public class AssertTests
    {
        [Fact]
        public void CanAssert_Plugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<TestModule>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    })
                    .Assert.Plugin<MockCorePlugin>(p =>
                    {
                        p.Modules.Should().HaveCount(1);
                    });
            });
        }
        
        [Fact]
        public void CanAssert_PluginModule()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<TestModule>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    })
                    .Assert.PluginModule<TestModule>(m =>
                    {
                        m.TestValue.Should().Be(800);
                    });
            });
        }

        [Fact]
        public void CanAssert_ServiceCollection()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<TestModule>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    })
                    .Assert.ServiceCollection(sc =>
                    {
                        var registration = sc.FirstOrDefault(sd => sd.ImplementationType == typeof(TestService));
                        registration.Should().NotBeNull();
                    });
            });
        }
        
        //-- Types used by unit-tests:
        
        private class TestModule : PluginModule
        {
            public int TestValue { get; }

            public TestModule()
            {
                TestValue = 800;
            }
            
            public override void RegisterServices(IServiceCollection services)
            {
                services.AddScoped<ITestService, TestService>();
            }
        }
        
        private interface ITestService
        {
            void SetValue(string value);
            string GetValue();
        }

        public class TestService : ITestService
        {
            private string _value;

            public void SetValue(string value) => _value = value;
            public string GetValue() => _value;
        }
    }
}