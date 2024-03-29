﻿using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Core.UnitTests.Bootstrap.Mocks;

namespace NetFusion.Core.UnitTests.Bootstrap;

/// <summary>
/// The following tests the constraints that must exist when composing a
/// CompositeContainer and its structure from a set of plugins. 
/// </summary>
public class CompositeContainerPluginTests
{
    /// <summary>
    /// The composite application will be constructed from one plugin that hosts
    /// the application.
    /// </summary>
    [Fact]
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
    /// plugin components.  These plugins contain the application specific components.
    /// </summary>
    [Fact]
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
    /// These plugins provide reusable components for technical implementations
    /// optionally used by other plugins.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// If a plugin is added more than once to the CompositeContainer, it is ignored.
    /// This allows a core plugin to register any dependent plugins without having to
    /// worry if another core plugin already registered the dependent plugin.
    /// </summary>
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
    [Fact]
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
        
    /// <summary>
    /// A plugin module of a specific type can only be registered once with a plugin.
    /// </summary>
    [Fact]
    public void PluginModule_CanOnlyBe_AddedOnce()
    {
        var exception = Assert.Throws<BootstrapException>(() =>
        {
            var plugin = new MockAppPlugin();
            plugin.AddModule<MockPluginOneModule>();
            plugin.AddModule<MockPluginOneModule>();
        });

        exception.ExceptionId.Should().Be("bootstrap-duplicate-module");
    }
        
    /// <summary>
    /// Plugins can register plugin-configurations used by the host to specify plugin specific settings.
    /// The CompositeContainer allows referencing any plugin configuration across all plugins registered
    /// with the container.
    /// </summary>
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
        
    /// <summary>
    /// If a requested plugin configuration is not registered, the composite
    /// container will raise an exception.
    /// </summary>
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
                .Assert.Exception<BootstrapException>(
                    ex => ex.ExceptionId.Should().Be("missing-plugin-config"));
        });
    }
        
    /// <summary>
    /// A given configuration can only be registered once with the plugin.
    /// </summary>
    [Fact]
    public void PluginConfig_CanOnlyBe_AddedOnce()
    {
        var exception = Assert.Throws<BootstrapException>(() =>
        {
            var plugin = new MockAppPlugin();
            plugin.AddConfig<MockPluginConfigOne>();
            plugin.AddConfig<MockPluginConfigOne>();
        });

        exception.ExceptionId.Should().Be("bootstrap-duplicate-config");
    }

    /// <summary>
    /// If a reference to a specific plugin is available, it can be used
    /// to request a registered configuration specific to the plugin. 
    /// </summary>
    [Fact]
    public void ExistingPluginConfig_CanBe_Requested()
    {
        var plugin = new MockAppPlugin();
        plugin.AddConfig<MockPluginConfigOne>();
            
        var config = plugin.GetConfig<MockPluginConfigOne>();
        config.Should().NotBeNull();
    }
        
    /// <summary>
    /// If a reference to a plugin is used to request a specific
    /// plugin configuration that does not exist, an exception is
    /// raised.
    /// </summary>
    [Fact]
    public void Requested_PluginConfig_MustExist()
    {
        var exception = Assert.Throws<BootstrapException>(() =>
        {
            var plugin = new MockAppPlugin();
            plugin.GetConfig<MockPluginConfigOne>();
        });

        exception.ExceptionId.Should().Be("missing-plugin-config");
    }
}