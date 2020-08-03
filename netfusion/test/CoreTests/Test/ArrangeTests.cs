using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Test
{
    /// <summary>
    /// These test show how to arrange a composite application container than can be tested.
    /// In a real running host application, these actions happen within the startup class
    /// by using ICompositeContainerBuilder.  Also, several of the Assert methods are shown.
    /// </summary>
    public class ArrangeTests
    {
        /// <summary>
        /// When testing a plugin implementation, the plugin under test needs to be added
        /// to the composite container.  Also, additional other mocked plugins can be added
        /// to test interactions between the plugin under test and other plugins that are
        /// usually part of the application being developed.
        /// </summary>
        [Fact]
        public void CanArrange_CompositeContainer()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugin<MockCorePlugin>();
                    }).Act.ComposeContainer()
                    .Assert.CompositeAppBuilder(b =>
                    {
                        b.AllPlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// When testing plugin having a component dependent on another service component,
        /// it is useful to arrange a service that can be used to vary the behavior of the
        /// component being tested.  Any services added here will override the corresponding
        /// service adding by any plugins.
        /// </summary>
        [Fact]
        public void CanArrange_Services()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Services(s =>
                    {
                        var service = new TestService();
                        service.SetTestValue("Arranged_Value");

                        s.AddSingleton<ITestService>(service);
                    })
                    .Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Assert.Service<ITestService>(s =>
                    {
                        s.GetTestValue().Should().Be("Arranged_Value");
                    });
            });
        }

        /// <summary>
        /// When testing a plugin, it is often useful to vary the a plugin's configuration
        /// so the impact on the plugin, and its related components, can be asserted.
        /// </summary>
        [Fact]
        public void CanArrange_PluginConfig()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        // NOTE:  This test is adding a configuration to the module.
                        // However, when testing a non-mocked plugin, the configuration
                        // is added internally and why you can arrange configurations.
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddConfig<MockPluginConfig>();
                        
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(corePlugin);
                    })
                    .PluginConfig((MockPluginConfig config) =>
                    {
                        config.ConfigValue = "Arranged-Value";
                    }).Act.ComposeContainer()
                    .Assert.Configuration((MockPluginConfig c) =>
                    {
                        c.ConfigValue.Should().Be("Arranged-Value");
                    });
            });
        }
        
        
        //-- Types used by unit-tests:
        
        private interface ITestService
        {
            public void SetTestValue(string value);
            public string GetTestValue();
        }

        private class TestService : ITestService
        {
            private string _value;

            public void SetTestValue(string value) => _value = value;
            public string GetTestValue() => _value;
        }
    }
}