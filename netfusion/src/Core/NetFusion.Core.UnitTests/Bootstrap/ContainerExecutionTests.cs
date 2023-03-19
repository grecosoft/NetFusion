using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Core.UnitTests.Bootstrap.Mocks;

namespace NetFusion.Core.UnitTests.Bootstrap;

public class ContainerExecutionTests
{
    /// <summary>
    /// When developing a plug-in that has associated configurations, they are most often
    /// accessed from within one or more modules.
    /// </summary>
    [Fact]
    public void Module_CanAccess_ConfigurationFromPlugin()
    {
        ContainerFixture.Test(fixture => {
            fixture.Arrange.Container(c =>
                {
                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddConfig<MockPluginConfigOne>();
                    hostPlugin.AddModule<MockPluginOneModule>();
                        
                    c.RegisterPlugins(hostPlugin);
                })
                .Assert.PluginModule<MockPluginOneModule>(m => {
                    m.Context.Plugin.GetConfig<MockPluginConfigOne>().Should().NotBeNull();
                });
        });
    }
        
    [Fact]
    public void Container_CanOnlyBeStartedOnce()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    c.RegisterPlugin<MockHostPlugin>();
                })
                .Act.RecordException().OnCompositeApp(c =>
                {
                    c.Start();
                    c.Start();
                })
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.Message.Should().Contain("Composite Application already started");
                });
        });
    }

    /// <summary>
    /// After the application container is built and started, each plug-in
    /// modules is given an opportunity to start.  This is where each 
    /// module is allowed to initialize any runtime services.  For example,
    /// this is where a service bus plug-in would create the needed queues
    /// and subscribe to consumers.
    /// </summary>
    [Fact]
    public void WhenAppContainerStarted_EachPluginModuleStarted()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddModule<MockPluginOneModule>();
                        
                    var corePlugin = new MockCorePlugin();
                    corePlugin.AddModule<MockPluginTwoModule>();
                        
                    c.RegisterPlugins(hostPlugin, corePlugin);
                })
                .Act.OnCompositeApp(ca =>
                {
                    ca.Start();
                })
                .Assert.CompositeAppBuilder(ca =>
                {
                    ca.AllModules.OfType<MockPluginModule>()
                        .All(m => m.IsStarted).Should().BeTrue();
                });
        });
    }

    /// <summary>
    /// When the application container is stopped, each plug-in module
    /// will be stopped.
    /// </summary>
    [Fact]
    public void AppContainerStopped_PluginModules_AreStopped()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddModule<MockPluginOneModule>();
                        
                    c.RegisterPlugins(hostPlugin);
                })
                .Act.OnCompositeApp(ca =>
                {
                    ca.Start();
                    ca.Stop();
                })
                .Assert.CompositeAppBuilder(ca =>
                {
                    var pluginModule = (MockPluginModule)ca.HostPlugin.Modules.First();
                    pluginModule.IsStopped.Should().BeTrue();
                });
        });
    }

    /// <summary>
    /// Once the application container is disposed, the associated service
    /// provider can no longer be accessed.
    /// </summary>
    [Fact]
    public void StoppedAppContainer_CannotHave_ServicesAccessed()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    c.RegisterPlugin<MockHostPlugin>();
                })
                .Act.RecordException().OnCompositeApp(ca =>
                {
                    ca.Start();
                    ca.Stop();
                    ca.CreateServiceScope();
                })
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.Message.Should().Be(
                        "Stopped Composite Application can not be accessed");
                });
        });
    }
}