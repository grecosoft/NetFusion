using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Testing;
using NetFusion.Tests.Core.Bootstrap.Mocks;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Core.Bootstrap
{
    /// <summary>
    /// The host application can register container configurations during the 
    /// bootstrap process.  All container configurations belong to a specific
    /// plug-in.  When the application is composed, all provided configurations
    /// are associated with the plug-in defining them.  The configurations can
    /// be referenced within the plug-in modules.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// Host application can provide an instance of a configuration object.
        /// </summary>
        [Fact]
        public void WhenConfigSpecified_AssociatedWithPlugin()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act(c =>
                {
                    c.WithConfig(new MockPluginConfig());
                    c.Build();
                })
                .Assert((CompositeApplication ca) =>
                {
                    ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);
                    ca.AppHostPlugin.PluginConfigs.First().Should().BeOfType<MockPluginConfig>();
                });
        }

        /// <summary>
        /// The host application can also provide a configuration by using factory method.   
        /// </summary>
        [Fact]
        public void WhenConfigFactoryUsed_ConfigAssociatedWithPlugin()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockPluginConfig>();
                })
                .Act(c =>
                {
                    c.WithConfig<MockPluginConfig>((confg) => { });
                    c.Build();
                })
                .Assert((CompositeApplication ca) =>
                {
                    ca.AppHostPlugin.PluginConfigs.Should().HaveCount(1);
                    ca.AppHostPlugin.PluginConfigs.First().Should().BeOfType<MockPluginConfig>();
                });
        }
    }
}
