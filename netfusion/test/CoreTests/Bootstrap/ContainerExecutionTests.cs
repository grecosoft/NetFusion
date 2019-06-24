using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    public class ContainerExecutionTests
    {
        [Fact(DisplayName = "Container can only be Started Once")]
        public void Container_CanOnlyBeStartedOnce()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnNonInitContainer(c =>
                    {
                        c.Compose(new TestTypeResolver())
                            .CreateServiceProvider();
                        
                        c.Start();
                        c.Start();
                    })
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("The application container has already been started.");
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
        [Fact(DisplayName = "When Container Started each Plug-in Module is Started")]
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
                    .Assert.CompositeApp(ca =>
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
        [Fact(DisplayName = "When Container stopped plug-in Modules are stopped")]
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
                    .Act.OnContainer(c =>
                    {
                        c.Stop();
                    })
                    .Assert.CompositeApp(ca =>
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
        [Fact(DisplayName = "Stopped Container cannot have Service Provider accessed")]
        public void StoppedAppContainer_CannotHave_ServicesAccessed()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnContainer(c =>
                    {
                        c.Stop();
                        c.CreateServiceScope();
                    })
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Be(
                            "The application container has been stopped and can no longer be accessed.");
                    });
            });
        }
    }
}