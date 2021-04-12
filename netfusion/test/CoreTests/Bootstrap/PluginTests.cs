using System.ComponentModel;
using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The following tests the constraints that must exist when composing a
    /// CompositeContainer and its structure from a set of plugins. 
    /// </summary>
    public class configPluginTests
    {
        /// <summary>
        /// The composite application will be constructed from one plugin that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application has Application Host Plugin")]
        public void Composite_Application_Has_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.HostPlugin.Should().NotBeNull();
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several application specific
        /// plugin components.  These plugins contain the main components of the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Application Component Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_AppComponentPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockAppPlugin());
                        c.RegisterPlugins(new MockAppPlugin());
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.AppPlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several core plugins.
        /// These plugins provide reusable services for technical implementations
        /// optionally used by other plugins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plugins")]
        public void CompositeApplication_CanHaveMultiple_CorePlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockCorePlugin());
                        c.RegisterPlugins(new MockCorePlugin());
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.CorePlugins.Should().HaveCount(2);
                    });
            });
        }

        [Fact]
        public void PluginIgnored_IfAdded_MoreThanOnce()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugin<MockCorePlugin>();
                        c.RegisterPlugin<MockCorePlugin>();
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.CorePlugins.Should().HaveCount(1);
                    });
            });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate and organize the implementation.
        /// </summary>
        [Fact(DisplayName = "Plug-In can have Multiple Modules")]
        public void PluginCanHave_MultipleModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        
                        var testPlugin = new MockCorePlugin();
                        testPlugin.AddModule<MockPluginTwoModule>();
                        testPlugin.AddModule<MockPluginThreeModule>();

                        c.RegisterPlugins(testPlugin);

                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        var pluginModules = ca.CorePlugins.First().Modules.ToArray();
                        pluginModules.Should().HaveCount(2);
                        pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                        pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                    });
            });
        }
        
        [Fact]
        public void CanReference_AnyPluginConfiguration()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        
                        var testCorePlugin = new MockCorePlugin();
                        testCorePlugin.AddConfig<MockPluginConfigOne>();

                        var testAppPlugin = new MockAppPlugin();
                        testAppPlugin.AddConfig<MockPluginConfigTwo>();

                        c.RegisterPlugins(testCorePlugin, testAppPlugin);

                    })
                    .Assert.CompositeContainer(c =>
                    {
                        c.GetPluginConfig<MockPluginConfigOne>().Should().NotBeNull();
                        c.GetPluginConfig<MockPluginConfigTwo>().Should().NotBeNull();
                    });
            });
        }
        
        [Fact]
        public void ReferencingAny_PluginConfiguration_MustExist()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        
                        var testCorePlugin = new MockCorePlugin();
                        testCorePlugin.AddConfig<MockPluginConfigOne>();

                        c.RegisterPlugins(testCorePlugin);

                    })
                    .Act.RecordException().OnCompositeContainer(c =>
                    {
                        c.GetPluginConfig<MockPluginConfigTwo>();
                    })
                    .Assert.Exception<ContainerException>(
                        ex => ex.ExceptionId.Should().Be("missing-plugin-config"));
            });
        }
        
        [Fact]
        public void PluginConfig_CanOnlyBe_AddedOnce()
        {
            var exception = Assert.Throws<ContainerException>(() =>
            {
                var plugin = new MockAppPlugin();
                plugin.AddConfig<MockPluginConfigOne>();
                plugin.AddConfig<MockPluginConfigOne>();
            });

            exception.ExceptionId.Should().Be("bootstrap-duplicate-config");
        }

        [Fact]
        public void Requested_PluginConfig_MustExist()
        {
            var exception = Assert.Throws<ContainerException>(() =>
            {
                var plugin = new MockAppPlugin();
                plugin.GetConfig<MockPluginConfigOne>();
            });

            exception.ExceptionId.Should().Be("missing-plugin-config");
        }
        
        [Fact]
        public void ExistingPluginConfig_CanBe_Requested()
        {
            var plugin = new MockAppPlugin();
            plugin.AddConfig<MockPluginConfigOne>();
            
            var config = plugin.GetConfig<MockPluginConfigOne>();
            config.Should().NotBeNull();
        }

        [Fact]
        public void PluginModule_CanOnlyBe_AddedOnce()
        {
            var exception = Assert.Throws<ContainerException>(() =>
            {
                var plugin = new MockAppPlugin();
                plugin.AddModule<MockPluginOneModule>();
                plugin.AddModule<MockPluginOneModule>();
            });

            exception.ExceptionId.Should().Be("bootstrap-duplicate-module");
        }
    }
}
