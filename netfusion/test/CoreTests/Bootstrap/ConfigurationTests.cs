using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The host application can register container configurations during the bootstrap process.  
    /// All container configurations belong to a specific plug-in.  When the application is composed, 
    /// all provided configurations are associated with the plug-in defining them.  The configurations 
    /// can be referenced within the plug-in modules.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// When developing a plug-in that has associated configurations, they are most often
        /// accessed from within one or more modules.
        /// </summary>
        [Fact(DisplayName = "Plug-in Developer can access Configuration from Module")]
        public void PluginDeveloper_CanAccess_ConfigurationFromModule()
        {
            ContainerFixture.Test(fixture => {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddConfig<MockPluginConfig>();
                        hostPlugin.AddModule<MockPluginOneModule>();
                        
                        c.RegisterPlugins(hostPlugin);
                    })
                    .Assert.PluginModule<MockPluginOneModule>(m => {
                        m.Context.Plugin.GetConfig<MockPluginConfig>().Should().NotBeNull();
                    });
            });
        }
    }
}
