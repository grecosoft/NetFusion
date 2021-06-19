using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// After a composite container is composed, a service named ICompositeApp is added to the
    /// service collection representing the application bootstrapped from the registered plugins.
    /// </summary>
    public class CompositeAppTests
    {
        [Fact]
        public void CompositeApp_Provides_SummaryOfHost()
        {
            ContainerFixture.Test(fixture =>
            {
                var hostPlugin = new MockHostPlugin();
                fixture.Arrange.Container(c => 
                {
                    c.RegisterPlugins(hostPlugin);
                })
                .Assert.CompositeApp(ca =>
                {
                    ca.HostPlugin.Should().NotBeNull();
                    ca.HostPlugin.PluginId.Should().Be(hostPlugin.PluginId);
                    ca.HostPlugin.Name.Should().Be(hostPlugin.Name);
                    ca.HostPlugin.AssemblyName.Should().Be(hostPlugin.AssemblyName);
                    ca.HostPlugin.AssemblyVersion.Should().Be(hostPlugin.AssemblyVersion);
                });
            });
        }

        [Fact]
        public void CompositeApp_Provides_CompositeLog()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugin<MockAppPlugin>();

                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<MockPluginOneModule>();
                        c.RegisterPlugins(corePlugin);
                    })
                    .Act.OnApplication(a => a.Start())
                    .Assert.CompositeApp(ca =>
                    {
                        var log = ca.Log;

                        log.Should().NotBeNull();
                    });
            });
        }
    }
}
